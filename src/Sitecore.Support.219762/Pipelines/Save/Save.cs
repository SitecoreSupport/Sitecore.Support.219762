namespace Sitecore.Support.Pipelines.Save
{
  using Data.Fields;
  using Data.Items;
  using Diagnostics;
  using Globalization;
  using Sitecore.Configuration;
  using Sitecore.Pipelines.Save;
  using Sitecore.Web;
  public class Save
  {
    #region Public methods

    /// <summary>
    /// Runs the processor.
    /// </summary>
    /// <param name="args">
    /// The arguments.
    /// </param>
    public void Process(SaveArgs args)
    {
      foreach (SaveArgs.SaveItem saveItem in args.Items)
      {
        Item item = Context.ContentDatabase.Items[saveItem.ID, saveItem.Language, saveItem.Version];

        if (item == null)
        {
          continue;
        }

        if (item.Locking.IsLocked())
        {
          if (!item.Locking.HasLock() && !Context.User.IsAdministrator && !args.PolicyBasedLocking)
          {
            args.Error = "Could not modify locked item \"" + item.Name + "\"";
            args.AbortPipeline();
            return;
          }
        }

        item.Editing.BeginEdit();

        foreach (SaveArgs.SaveField saveField in saveItem.Fields)
        {
          Field field = item.Fields[saveField.ID];

          if (field == null ||
              (field.IsBlobField
               && (field.TypeKey == "attachment" || saveField.Value == Translate.Text(Texts.BLOB_VALUE))))
          {
            // TODO: Insert more proper logic 
            continue;
          }

          if (field.Type.Equals("Password", System.StringComparison.OrdinalIgnoreCase) && string.IsNullOrEmpty(saveField.Value))
          {
            continue;
          }

          saveField.OriginalValue = field.Value;

          if (saveField.OriginalValue == saveField.Value)
          {
            continue;
          }

          if (!string.IsNullOrEmpty(saveField.Value))
          {
            if (field.TypeKey == "rich text" && Settings.HtmlEditor.RemoveScripts)
            {
              saveField.Value = WebUtil.RemoveAllScripts(saveField.Value);
            }

            if (NeedsHtmlTagEncode(saveField))
            {
              saveField.Value = saveField.Value.Replace("<", "&lt;").Replace(">", "&gt;");
            }
          }

          field.Value = saveField.Value;
        }

        item.Editing.EndEdit();

        Log.Audit(this, "Save item: {0}", AuditFormatter.FormatItem(item));

        args.SavedItems.Add(item);
      }

      if (!Context.IsUnitTesting)
      {
        Context.ClientPage.Modified = false;
      }

      if (args.SaveAnimation)
      {
        Context.ClientPage.ClientResponse.Eval("var d = new scSaveAnimation('ContentEditor')");
      }
    }

    #endregion

    #region Pivate methods

    /// <summary>
    /// Defines if the html tags in the field value should be encoded.
    /// </summary>
    /// <param name="field">The save field</param>
    /// <returns><c>true</c> if the value should be encoded and <c>false</c> otherwise.</returns>
    private static bool NeedsHtmlTagEncode([NotNull] SaveArgs.SaveField field)
    {
      return field.ID == FieldIDs.DisplayName;
    }

    #endregion
  }
}