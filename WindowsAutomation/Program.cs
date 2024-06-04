using System;
using System.Windows.Automation;
using System.Linq;
using System.Collections.Generic;
using System.Windows.Forms;
using System.Xml.Linq;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.TextBox;
using System.Threading;

namespace WindowsAutomation
{
    class TreeNode
    {
        public AutomationElement node;
        public List<TreeNode> children;
    }

    internal class Program
    {
        static void Main(string[] args)
        {
            var parentElement = GetSiemensParentElement();
            var ancestor = GetElementFromChildList(parentElement, new List<int> { 0 }, 0);
            Thread.Sleep(5000);
            var element = ElementFromCursor();
            var click = element.GetClickablePoint();
            Console.WriteLine("Stop here");
        }

        public static AutomationElement GetSiemensParentElement()
        {
            AutomationElement siemensParentElement = null;
            for (var i = 0; i < 60; i++)
            {
                // If the UI changes while retrieving the parent element, an error will throw. This try/catch loop tries to get the UI element
                // as quickly as possible. If the UI is changes, wait a second, and then try again.
                // This loop will timeout after one minute, which is probably longer than needed. Future work: move timeout to job timeout. 
                try
                {
                    AutomationElementCollection allChildren =
                              AutomationElement.RootElement.FindAll(TreeScope.Children, Condition.TrueCondition);

                    foreach (AutomationElement element in allChildren)
                    {
                        if (element.Current.Name.Contains("ADWork"))
                        {
                            siemensParentElement = element;
                            break;
                        }
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.ToString());
                    Thread.Sleep(1000);
                    continue;
                }
            }
            if (siemensParentElement == null)
            {
                throw new Exception("Unable to get Siemens parent UI element within one minute");
            }
            return siemensParentElement;
        }

        public static AutomationElement GetElementFromChildList(AutomationElement parent, List<int> childIndexList, int index)
        {
            var childIndex = childIndexList[index];
            var childList = new List<AutomationElement>();
            if (childIndex == 0)
            {
                var child = parent.FindFirst(TreeScope.Children, Condition.TrueCondition);
                childList.Add(child);
            }
            else
            {
                foreach (AutomationElement child in parent.FindAll(TreeScope.Children, Condition.TrueCondition))
                {
                    childList.Add(child);
                }
            }

            if (childIndex >= childList.Count)
            {
                throw new ArgumentOutOfRangeException($"Could not find element in list. childIndex ({childIndex}) out of range for childList ({childList.Select(child => child.Current.Name).ToList()})");
            }

            var selectedChild = childList[childIndex];

            if (index == childIndexList.Count - 1)
            {
                return selectedChild;
            }

            return GetElementFromChildList(selectedChild, childIndexList, index + 1);
        }

        public static AutomationElement ElementFromCursor()
        {
            // Convert mouse position from System.Drawing.Point to System.Windows.Point.
            System.Windows.Point point = new System.Windows.Point(Cursor.Position.X, Cursor.Position.Y);
            AutomationElement element = AutomationElement.FromPoint(point);
            return element;
        }

        public static TreeNode CreateTree(AutomationElement parent)
        {
            var node = parent;
            var childList = new List<AutomationElement>();
            foreach (AutomationElement child in parent.FindAll(TreeScope.Children, Condition.TrueCondition))
            {
                childList.Add(child);
            }

            var children = childList.Select(child => CreateTree(child)).ToList();

            return new TreeNode { node = node, children = children };
        }

        static public AutomationElement GetElementFromName(string controlName, AutomationElement rootElement)
        {
            if ((controlName == "") || (rootElement == null))
            {
                throw new ArgumentException("Argument cannot be null or empty.");
            }
            // Set a property condition that will be used to find the main form of the
            // target application. In the case of a WinForms control, the name of the control
            // is also the AutomationId of the element representing the control.
            Condition propCondition = new PropertyCondition(
                AutomationElement.NameProperty, controlName, PropertyConditionFlags.IgnoreCase);

            // Find the element.
            return rootElement.FindFirst(TreeScope.Descendants | TreeScope.Children, propCondition);
        }

        static public AutomationElement GetElementFromProperty(string controlName, AutomationElement rootElement, AutomationProperty property, TreeScope scope)
        {
            if ((controlName == "") || (rootElement == null))
            {
                throw new ArgumentException("Argument cannot be null or empty.");
            }
            // Set a property condition that will be used to find the main form of the
            // target application. In the case of a WinForms control, the name of the control
            // is also the AutomationId of the element representing the control.
            Condition propCondition = new PropertyCondition(
                property, controlName, PropertyConditionFlags.IgnoreCase);

            // Find the element.
            return rootElement.FindFirst(scope, propCondition);
        }
    }
}


