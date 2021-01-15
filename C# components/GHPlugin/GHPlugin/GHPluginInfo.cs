using System;
using System.Drawing;
using Grasshopper.Kernel;

namespace GHPlugin
{
    public class GHPluginInfo : GH_AssemblyInfo
    {
        public override string Name
        {
            get
            {
                return "GHPlugin";
            }
        }
        public override Bitmap Icon
        {
            get
            {
                //Return a 24x24 pixel bitmap to represent this GHA library.
                return null;
            }
        }
        public override string Description
        {
            get
            {
                //Return a short string describing the purpose of this GHA library.
                return "";
            }
        }
        public override Guid Id
        {
            get
            {
                return new Guid("d5f98797-ab8a-4cf1-95cf-5df97276327a");
            }
        }

        public override string AuthorName
        {
            get
            {
                //Return a string identifying you or your company.
                return "";
            }
        }
        public override string AuthorContact
        {
            get
            {
                //Return a string representing your preferred contact details.
                return "";
            }
        }
    }
}
