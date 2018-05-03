namespace Sitecore.Support.Shell.Applications.ContentEditor
{
  using Sitecore.Diagnostics;
  using System.Text;
  using System.Web.UI;
  public class Password : Sitecore.Shell.Applications.ContentEditor.Password
  {
    protected override void DoRender(HtmlTextWriter output)
    {
      int valueLen = 0;

      try
      {
        valueLen = base.Attributes["value"] == null ? 0 : base.Attributes["value"].Length;
      }
      catch
      {
        Log.Warn("Sitecore.Support.219762: Could not resolve password length.", this);
      }

      base.Attributes["placeholder"] = new StringBuilder().Insert(0, "\u2022", valueLen).ToString(); ;
      base.Attributes["value"] = string.Empty;
      string str = Password ? " type=\"password\"" : (Hidden ? " type=\"hidden\"" : "");
      base.SetWidthAndHeightStyle();

      output.Write("<input" + base.ControlAttributes + str + ">");
      RenderChildren(output);
    }

    protected override void SetModified()
    {
      if (base.Attributes["value"] != string.Empty)
        base.SetModified();
    }
  }
}