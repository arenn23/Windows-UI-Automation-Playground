using System;
using System.Windows.Automation;
using System.Linq;
using System.Collections.Generic;
using System.Windows.Forms;
using System.Xml.Linq;

namespace WindowsAutomation
{
    class TreeNode
    {
        public AutomationElement node;
        public string path;
        public List<TreeNode> children;
    }
    internal class Program
    {
        static void Main(string[] args)
        {
            AutomationElementCollection allChildren =
                AutomationElement.RootElement.FindAll(TreeScope.Children, Condition.TrueCondition);

            List<AutomationElement> selectedChildren = new List<AutomationElement>();
            foreach (AutomationElement element in allChildren)
            {
                if (element.Current.Name.Contains("AD"))
                {
                    selectedChildren.Add(element);
                }
            }

            var thisOne = FindChildElement("Application.ExtendedUpload_ICO_PE_gTbLoadToPG", selectedChildren[0]);
            Console.WriteLine("here");
        }

        static private AutomationElement FindChildElement(String controlName, AutomationElement rootElement)
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
    }
}

