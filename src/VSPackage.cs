using System;
using System.ComponentModel.Design;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows;
using EnvDTE;
using EnvDTE80;
using Microsoft;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Text.Editor;
using Task = System.Threading.Tasks.Task;

namespace MadsKristensen.TextGenerator
{

    [PackageRegistration(UseManagedResourcesOnly = true, AllowsBackgroundLoading = true)]
    [InstalledProductRegistration("#110", "#112", Vsix.Version, IconResourceID = 400)]
    [ProvideMenuResource("Menus.ctmenu", 1)]
    [Guid(PackageGuids.guidTextGeneratorPkgString)]
    public sealed class VSPackage : AsyncPackage
    {
        protected override async Task InitializeAsync(CancellationToken cancellationToken, IProgress<ServiceProgressData> progress)
        {
            await JoinableTaskFactory.SwitchToMainThreadAsync();

            var mcs = await GetServiceAsync(typeof(IMenuCommandService)) as OleMenuCommandService;
            Assumes.Present(mcs);

            var commandId = new CommandID(PackageGuids.guidTextGeneratorCmdSet, PackageIds.cmdGenerate);
            var command = new MenuCommand(Execute, commandId);
            mcs.AddCommand(command);
        }

        private void Execute(object sender, EventArgs e)
        {
            IWpfTextView view = ProjectHelpers.GetCurentTextView();

            if (view != null)
            {
                var dte = (DTE2)GetService(typeof(DTE));
                var text = GetText();

                if (!string.IsNullOrEmpty(text))
                {
                    InsertText(view, dte, text);
                }
            }
        }

        private string GetText()
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            var dialog = new GeneratorDialog(this, 0)
            {
                Owner = Application.Current.MainWindow
            };

            var result = dialog.ShowDialog();

            if (result.HasValue && result.Value)
            {
                if (result.HasValue && result.Value)
                {
                    return dialog.Text;
                }
            }

            return null;
        }

        private static void InsertText(IWpfTextView view, DTE2 dte, string text)
        {
            ThreadHelper.ThrowIfNotOnUIThread();

            try
            {
                dte.UndoContext.Open("Generate text");

                using (Microsoft.VisualStudio.Text.ITextEdit edit = view.TextBuffer.CreateEdit())
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
                System.Diagnostics.Debug.Write(ex);
            }
            finally
            {
                dte.UndoContext.Close();
            }
        }
    }
}
