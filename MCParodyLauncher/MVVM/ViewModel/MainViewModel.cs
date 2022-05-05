using MCParodyLauncher.Core;
using System;

namespace MCParodyLauncher.MVVM.ViewModel
{
    internal class MainViewModel : ObservableObject
    {

        public RelayCommand HomeViewCommand { get; set; }
        public RelayCommand Minecraft2ViewCommand { get; set; }
        public RelayCommand Minecraft3ViewCommand { get; set; }
        public RelayCommand Minecraft4ViewCommand { get; set; }

        public HomeViewModel HomeVM { get; set; }
        
        public Minecraft2ViewModel Minecraft2VM { get; set; }
        public Minecraft3ViewModel Minecraft3VM { get; set; }
        public Minecraft4ViewModel Minecraft4VM { get; set; }

        private object _currentView;

        public object CurrentView
        {
            get { return _currentView; }
            set
            {
                _currentView = value;
                OnPropertyChanged();
            }
        }

        public MainViewModel()
        {
            HomeVM = new HomeViewModel();
            Minecraft2VM = new Minecraft2ViewModel();
            Minecraft3VM = new Minecraft3ViewModel();
            Minecraft4VM = new Minecraft4ViewModel();
            CurrentView = HomeVM;

            HomeViewCommand = new RelayCommand(o =>
            {
                CurrentView = HomeVM;
            });
            Minecraft2ViewCommand = new RelayCommand(o =>
            {
                CurrentView = Minecraft2VM;
            });
            Minecraft3ViewCommand = new RelayCommand(o =>
            {
                CurrentView = Minecraft3VM;
            });
            Minecraft4ViewCommand = new RelayCommand(o =>
            {
                CurrentView = Minecraft4VM;
            });
        }
    }
}
