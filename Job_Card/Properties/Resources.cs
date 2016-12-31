namespace Job_Card.Properties
{
    using System;
    using System.CodeDom.Compiler;
    using System.ComponentModel;
    using System.Diagnostics;
    using System.Drawing;
    using System.Globalization;
    using System.Resources;
    using System.Runtime.CompilerServices;

    [DebuggerNonUserCode, GeneratedCode("System.Resources.Tools.StronglyTypedResourceBuilder", "4.0.0.0"), CompilerGenerated]
    internal class Resources
    {
        private static CultureInfo resourceCulture;
        private static System.Resources.ResourceManager resourceMan;

        internal Resources()
        {
        }

        [EditorBrowsable(EditorBrowsableState.Advanced)]
        internal static CultureInfo Culture
        {
            get
            {
                return resourceCulture;
            } 
 
            set
            {
                resourceCulture = value;
            }
        }

        internal static Bitmap logo =>
            ((Bitmap) ResourceManager.GetObject("logo", resourceCulture));

        internal static Bitmap logoHalfSize =>
            ((Bitmap) ResourceManager.GetObject("logoHalfSize", resourceCulture));

        internal static Bitmap paid_stamp =>
            ((Bitmap) ResourceManager.GetObject("paid_stamp", resourceCulture));

        internal static Bitmap paidSmall =>
            ((Bitmap) ResourceManager.GetObject("paidSmall", resourceCulture));

        [EditorBrowsable(EditorBrowsableState.Advanced)]
        internal static System.Resources.ResourceManager ResourceManager
        {
            get
            {
                if (object.ReferenceEquals(resourceMan, null))
                {
                    System.Resources.ResourceManager manager = new System.Resources.ResourceManager("Job_Card.Properties.Resources", typeof(Resources).Assembly);
                    resourceMan = manager;
                }
                return resourceMan;
            }
        }
    }
}

