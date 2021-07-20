using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using System.IO;
using RPA_Workbench.Views;
using System.Windows;

namespace RPA_Workbench.ViewModels.ProjectSolutionControls
{
    public class RenameDialogViewModel
    {
        Views.ProjectSolutionControls.RenameDialog renameDialogLocal;
        public RenameDialogViewModel()
        {
            RelaySetup();
        }
        public RenameDialogViewModel(Views.ProjectSolutionControls.RenameDialog renameDialog = null)
        {
            renameDialogLocal = renameDialog;
            RelaySetup();
        }
        void RelaySetup()
        {
            RenameCommand = new RelayCommand(new Action<object>(RenameProject));
            CloseWindowCommand = new RelayCommand(new Action<object>(CloseWindow));
        }

        #region Icommands
        ICommand iRename;
        public ICommand RenameCommand {
            get { return iRename; }
            set { iRename = value; }
        }

        ICommand iCloseWindow;
        public ICommand CloseWindowCommand
        {
            get { return iCloseWindow; }
            set { iCloseWindow = value; }
        }
        #endregion

        #region Methods/Functions
        void RenameProject(object parameters)
        {
            string NewFileNameFullPath;
            string OldFileNameFullPath = ParentFolder + "\\" + OldFileName;
            if (File.Exists(ParentFolder + "\\" + NewFileName + ".xaml") == true)
            {
                var messageBoxResult = CustomControls.Views.CustomMessageBox.Show("Path already exists", "File with same name exits, choose another name",
                CustomControls.Views.CustomMessageBox.MessageBoxButtons.OK);
            }
            else
            {
                if (NewFileName.Contains(".xaml")) // If the user inserts .xaml with the name, it will remove it
                {
                    NewFileName = NewFileName.Replace(".xaml", "");
                }
                NewFileNameFullPath = ParentFolder + "\\" + NewFileName + ".xaml";
                File.Move(OldFileNameFullPath, NewFileNameFullPath);
                renameDialogLocal.Close();
            }
           

        }

        void CloseWindow(object parameters) {
            renameDialogLocal.Close();
        }
        #endregion

        #region Public Methods/Functions

        #endregion

        #region Public Properties
        string _parentFolder;
        public string ParentFolder
        {
            get { return _parentFolder; }
            set { _parentFolder = value; }
        }

        string _oldFilename;
        public string OldFileName {
            get { return _oldFilename; }
            set { _oldFilename = value; }
        }

        string _newFilename;
        public string NewFileName {
            get { return _newFilename; }
            set { _newFilename = value; }
        }
        #endregion
    }
}
