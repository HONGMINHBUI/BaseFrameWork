namespace OCC.UI.TestingFramework.Utility
{
    public static class XpathHelper
    {
        /// <summary>
        /// Translates a jquery selector to an Xpath selector
        /// It can handle:
        /// -class selector
        /// -id selector
        /// -attribute selector
        /// -if the selector is none of these above, it creates a "text" selector
        /// </summary>
        /// <param name="selector"></param>
        /// <returns></returns>
        public static string ToXPathSelector(string selector)
        {
            var selectorType =selector.Contains("=")?'a':selector[0];
            var identifier = selector.Substring(1);

            switch (selectorType)
            {
                case '#':
                    return string.Format("*[@id='{0}']", identifier);
                case '.':
                    return string.Format("*[contains(@class, '{0}')]", identifier);
                case 'a':
                    var tmp = selector.Split('=');
                    return string.Format("*[@{0}='{1}']", tmp[0],tmp[1]);
                default:
                    return string.Format("*[text()='{0}' or @value='{0}']", selector);
            }
        }
    }
}