﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.ComponentModelHost;
using Microsoft.VisualStudio.Editor;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.TextManager.Interop;

namespace MadsKristensen.TextGenerator
{
    static class ProjectHelpers
    {
        ///<summary>Gets the TextView for the active document.</summary>
        public static IWpfTextView GetCurentTextView()
        {
            var componentModel = GetComponentModel();
            if (componentModel == null) return null;
            var editorAdapter = componentModel.GetService<IVsEditorAdaptersFactoryService>();

            return editorAdapter.GetWpfTextView(GetCurrentNativeTextView());
        }

        public static IVsTextView GetCurrentNativeTextView()
        {
            var textManager = (IVsTextManager)ServiceProvider.GlobalProvider.GetService(typeof(SVsTextManager));

            IVsTextView activeView = null;
            ErrorHandler.ThrowOnFailure(textManager.GetActiveView(1, null, out activeView));
            return activeView;
        }

        public static IComponentModel GetComponentModel()
        {
            return (IComponentModel)Package.GetGlobalService(typeof(SComponentModel));
        }
    }
}