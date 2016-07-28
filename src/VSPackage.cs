using System;
using System.ComponentModel.Design;
using System.Runtime.InteropServices;
using System.Windows.Interop;
using EnvDTE;
using EnvDTE80;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio.Text.Editor;

namespace MadsKristensen.TextGenerator
{

    [PackageRegistration(UseManagedResourcesOnly = true)]
    [InstalledProductRegistration("#110", "#112", Vsix.Version, IconResourceID = 400)]
    [ProvideMenuResource("Menus.ctmenu", 1)]
    [ProvideAutoLoad(UIContextGuids80.SolutionExists)]
    [Guid(PackageGuids.guidTextGeneratorPkgString)]
    public sealed class VSPackage : Package
    {
        protected override void Initialize()
        {
            base.Initialize();

            Logger.Initialize(this, Vsix.Name);

            OleMenuCommandService mcs = GetService(typeof(IMenuCommandService)) as OleMenuCommandService;
            if (null != mcs)
            {
                CommandID menuCommandID = new CommandID(PackageGuids.guidTextGeneratorCmdSet, PackageIds.cmdGenerate);
                MenuCommand menuItem = new MenuCommand(Execute, menuCommandID);
                mcs.AddCommand(menuItem);
            }
        }

        private void Execute(object sender, EventArgs e)
        {
            var view = ProjectHelpers.GetCurentTextView();

            if (view != null)
            {
                var dte = (DTE2)GetService(typeof(DTE));
                string text = GetText(dte);

                if (!string.IsNullOrEmpty(text))
                    InsertText(view, dte, text);
            }
        }

        private string GetText(DTE2 dte)
        {
            GeneratorDialog dialog = new GeneratorDialog(this, 0);

            var hwnd = new IntPtr(dte.MainWindow.HWnd);
            System.Windows.Window window = (System.Windows.Window)HwndSource.FromHwnd(hwnd).RootVisual;
            dialog.Owner = window;

            var result = dialog.ShowDialog();

            if (result.HasValue && result.Value)
                return dialog.Text;

            return null;
        }

        private static void InsertText(IWpfTextView view, DTE2 dte, string text)
        {
            try
            {
                dte.UndoContext.Open("Generate text");

                using (var edit = view.TextBuffer.CreateEdit())
                {
                    if (!view.Selection.IsEmpty)
                    {
                        edit.Delete(view.Selection.SelectedSpans[0].Span);
                        view.Selection.Clear();
                    }

                    edit.Insert(view.Caret.Position.BufferPosition, text);
                    edit.Apply();
                }
            }
            catch (Exception ex)
            {
                Logger.Log(ex);
            }
            finally
            {
                dte.UndoContext.Close();
            }
        }
    }
}
