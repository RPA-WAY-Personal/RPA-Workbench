using System;
using System.Activities.Presentation.Metadata;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RPAWorkbench.UiAutomation.Activities
{
    public sealed class TestAutomationMetadata : IRegisterMetadata
    {
        public void Register()
        {
            RegisterAll();
        }

        public static void RegisterAll()
        {
            var builder = new AttributeTableBuilder();
            TestAutomationDesigner.RegisterMetadata(builder);
            MetadataStore.AddAttributeTable(builder.CreateTable());
        }
    }
}
