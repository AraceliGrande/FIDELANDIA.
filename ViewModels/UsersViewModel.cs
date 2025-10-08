using FIDELANDIA.Commands;
using FIDELANDIA.Models;
using FIDELANDIA.Services;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace FIDELANDIA.ViewModels 
{
    public class UsersViewModel : ViewModelBase
    {
        private readonly BaseDeDatos _baseDeDatos;
        private ObservableCollection<UserModel> _usuarios;
        private UserModel _usuarioSeleccionado;

        public UsersViewModel()
        {
            _baseDeDatos = new BaseDeDatos();
            _usuarioSeleccionado = new UserModel();
            _usuarios = _baseDeDatos.GetUsuarios();
        }

        public UserModel UsuarioSeleccionado
        {
            get => _usuarioSeleccionado;
            set
            {
                if (_usuarioSeleccionado != value)
                {
                    _usuarioSeleccionado = value;
                    OnPropertyChanged(nameof(UsuarioSeleccionado));
                }
            }
        }

        public ObservableCollection<UserModel> Usuarios
        {
            get => _usuarios;
            set
            {
                if (_usuarios != value)
                {
                    _usuarios = value;
                    OnPropertyChanged(nameof(Usuarios));
                }
            }
        }

        public ICommand AddCommand
        {
            get
            {
                return new RelayCommand(
                    (parameter) => AddUsuario((UserModel)parameter),
                    (parameter) => AddCanExecute((UserModel)parameter));
            }
        }

        public void AddUsuario(UserModel User)
        {
            _baseDeDatos.AddUsuario(User);
            Usuarios = _baseDeDatos.GetUsuarios();
        }
        public bool AddCanExecute(UserModel User)
        {
            return true;
        }
    }
}
