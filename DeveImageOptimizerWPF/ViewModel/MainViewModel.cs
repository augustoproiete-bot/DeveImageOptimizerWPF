using DeveImageOptimizer;
using DeveImageOptimizer.FileProcessing;
using DeveImageOptimizer.Helpers;
using DeveImageOptimizer.State;
using DeveImageOptimizerWPF.Helpers;
using DeveImageOptimizerWPF.State;
using DeveImageOptimizerWPF.State.MainWindowState;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using Ookii.Dialogs.Wpf;
using System.Collections.ObjectModel;
using System.IO;
using System.Threading.Tasks;
using System.Windows.Input;

namespace DeveImageOptimizerWPF.ViewModel
{
    /// <summary>
    /// This class contains properties that the main View can data bind to.
    /// <para>
    /// Use the <strong>mvvminpc</strong> snippet to add bindable properties to this ViewModel.
    /// </para>
    /// <para>
    /// You can also use Blend to data bind with the tool's support.
    /// </para>
    /// <para>
    /// See http://www.galasoft.ch/mvvm
    /// </para>
    /// </summary>
    public class MainViewModel : ViewModelBase
    {
        public WindowState WindowState { get; set; }
        public FilesProcessingState FilesProcessingState { get; set; }

        public MainViewModel()
        {
            WindowState = StaticState.WindowStateManager.State;
            FilesProcessingState = new FilesProcessingState();

            WindowState.PropertyChanged += ProcessingStateData_PropertyChanged;
            FilesProcessingState.PropertyChanged += FilesProcessingState_PropertyChanged;

            GoCommand = new RelayCommand(async () => await GoCommandImp(), () => true);
            BrowseCommand = new RelayCommand(() => BrowseCommandImp(), () => true);
        }

        private void FilesProcessingState_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            //StaticState.FilesProcessingStateManager.Save();
        }

        private void ProcessingStateData_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            StaticState.WindowStateManager.Save();
        }

        public ICommand GoCommand { get; private set; }
        private async Task GoCommandImp()
        {
            var state = StaticState.UserSettingsManager.State;

            var tempDir = Path.Combine(FolderHelperMethods.EntryAssemblyDirectory.Value, ConstantsAndConfig.TempDirectoryName);
            var fileOptimizer = new FileOptimizerProcessor(state.FileOptimizerPath, tempDir, !state.HideFileOptimizerWindow, state.LogLevel, state.SaveFailedFiles);
            var fileProcessor = new FileProcessor(fileOptimizer, FilesProcessingState, new FileProcessedStateRememberer(state.ForceOptimizeEvenIfAlreadyOptimized));

            if (!state.ExecuteImageOptimizationParallel)
            {
                await fileProcessor.ProcessDirectory(WindowState.ProcessingDirectory);
            }
            else
            {
                await fileProcessor.ProcessDirectoryParallel(WindowState.ProcessingDirectory, state.MaxDegreeOfParallelism);
            }
        }

        public ICommand BrowseCommand { get; private set; }
        private void BrowseCommandImp()
        {
            var folderDialog = new VistaFolderBrowserDialog();

            string startDir = InitialDirFinder.FindStartingDirectoryBasedOnInput(WindowState.ProcessingDirectory);
            if (Directory.Exists(startDir))
            {
                folderDialog.SelectedPath = startDir;
            }

            if (folderDialog.ShowDialog() == true)
            {
                WindowState.ProcessingDirectory = folderDialog.SelectedPath;
            }
        }
    }
}