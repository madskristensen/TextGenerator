using System;
using System.ComponentModel.Design;
using System.Runtime.InteropServices;
using EnvDTE;
using EnvDTE80;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;

namespace MadsKristensen.TextGenerator
{

    [PackageRegistration(UseManagedResourcesOnly = true)]
    [InstalledProductRegistration("#110", "#112", Version, IconResourceID = 400)]
    [ProvideMenuResource("Menus.ctmenu", 1)]
    [ProvideAutoLoad(UIContextGuids80.SolutionExists)]
    [Guid(PackageGuids.guidTextGeneratorPkgString)]
    public sealed class VSPackage : Package
    {
        public const string Version = "1.0";
        public const string Name = "Text Generator";

        protected override void Initialize()
        {
            base.Initialize();

            Telemetry.Initialize(this, Version, "d14d5404-e81e-477d-980e-87fac8281353");
            Logger.Initialize(this, Name);

            OleMenuCommandService mcs = GetService(typeof(IMenuCommandService)) as OleMenuCommandService;
            if (null != mcs)
            {
                CommandID menuCommandID = new CommandID(PackageGuids.guidTextGeneratorCmdSet, PackageIds.cmdidMyCommand);
                MenuCommand menuItem = new MenuCommand(InsertText, menuCommandID);
                mcs.AddCommand(menuItem);
            }
        }

        private void InsertText(object sender, EventArgs e)
        {
            var view = ProjectHelpers.GetCurentTextView();

            if (view == null)
                return;

            var dte = (DTE2)GetService(typeof(DTE));

            dte.UndoContext.Open("Generate text");

            try
            {
                GeneratorDialog dialog = new GeneratorDialog(this, 0);
                var result = dialog.ShowDialog();

                if (!result.HasValue || !result.Value)
                    return;

                using (var edit = view.TextBuffer.CreateEdit())
                {
                    if (!view.Selection.IsEmpty)
                    {
                        edit.Delete(view.Selection.SelectedSpans[0].Span);
                        view.Selection.Clear();
                    }

                    edit.Insert(view.Caret.Position.BufferPosition, dialog.Text);
                    edit.Apply();
                }
            }
            finally
            {
                dte.UndoContext.Close();
            }
        }
    }
}
