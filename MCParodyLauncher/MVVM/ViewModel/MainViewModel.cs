using MCParodyLauncher.Core;
using MCParodyLauncher.MVVM.View;
using System;

namespace MCParodyLauncher.MVVM.ViewModel
{
    internal class MainViewModel : ObservableObject
    {

        public RelayCommand HomeViewCommand { get; set; }
        public RelayCommand Minecraft2ViewCommand { get; set; }
        public RelayCommand Minecraft3ViewCommand { get; set; }
        public RelayCommand Minecraft4ViewCommand { get; set; }
        public RelayCommand Minecraft5ViewCommand { get; set; }

        public HomeViewModel HomeVM { get; set; }
        
        public Minecraft2ViewModel Minecraft2VM { get; set; }
        public Minecraft3ViewModel Minecraft3VM { get; set; }
        public Minecraft4ViewModel Minecraft4VM { get; set; }
        public Minecraft5ViewModel Minecraft5VM { get; set; }

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
            Minecraft5VM = new Minecraft5ViewModel();
            CurrentView = HomeVM;

            HomeViewCommand = new RelayCommand(o =>
            {
                if (Minecraft2View.downloadActive == false && Minecraft3View.downloadActive == false && Minecraft4View.downloadActive == false && Minecraft5View.downloadActive == false)
                {
                    CurrentView = HomeVM;
                }
            });
            Minecraft2ViewCommand = new RelayCommand(o =>
            {
                if (Minecraft2View.downloadActive == false && Minecraft3View.downloadActive == false && Minecraft4View.downloadActive == false && Minecraft5View.downloadActive == false)
                {
                    CurrentView = Minecraft2VM;
                }
            });
            Minecraft3ViewCommand = new RelayCommand(o =>
            {
                if (Minecraft2View.downloadActive == false && Minecraft3View.downloadActive == false && Minecraft4View.downloadActive == false && Minecraft5View.downloadActive == false)
                {
                    CurrentView = Minecraft3VM;
                }
            });
            Minecraft4ViewCommand = new RelayCommand(o =>
            {
                if (Minecraft2View.downloadActive == false && Minecraft3View.downloadActive == false && Minecraft4View.downloadActive == false && Minecraft5View.downloadActive == false)
                {
                    CurrentView = Minecraft4VM;
                }
            });
            Minecraft5ViewCommand = new RelayCommand(o =>
            {
                if (Minecraft2View.downloadActive == false && Minecraft3View.downloadActive == false && Minecraft4View.downloadActive == false && Minecraft5View.downloadActive == false)
                {
                    CurrentView = Minecraft5VM;
                }
            });
        }
    }
}
