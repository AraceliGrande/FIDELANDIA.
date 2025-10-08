using FIDELANDIA.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Data.SqlClient;

namespace FIDELANDIA.Services
{
    internal class BaseDeDatos
    {
        private readonly string _conexionString;

        public string ConexionString => _conexionString;

        public BaseDeDatos()
        {
            _conexionString = @"Server=DESKTOP-ML2Q34Q\SQLEXPRESS;Database=FIDELANDIA;Trusted_Connection=True;Encrypt=True;TrustServerCertificate=True;";
        }

        internal ObservableCollection<UserModel> GetUsuarios()
        {
            var usuarios = new ObservableCollection<UserModel>();
            string sql = "SELECT * FROM usuarios";

            using var conexion = new SqlConnection(_conexionString);
            conexion.Open();

            using var cmd = new SqlCommand(sql, conexion);
            using var reader = cmd.ExecuteReader();

            while (reader.Read())
            {
                usuarios.Add(new UserModel()
                {
                    Id = (int)reader["iduser"],
                    Username = (string)reader["username"],
                    Password = (string)reader["password"],
                    Email = (string)reader["email"]
                });
            }
            reader.Close();
            conexion.Close();

            return usuarios;
        }
        internal void AddUsuario(UserModel usuario)
        {
            string sql = "INSERT INTO usuarios (iduser, username, password, email) VALUES (@iduser, @username, @password, @email)";

            using var conexion = new SqlConnection(_conexionString);
            conexion.Open();

            using var cmd = new SqlCommand(sql, conexion);
            // Agregar parámetros para evitar SQL Injection
            cmd.Parameters.AddWithValue("@username", usuario.Username);
            cmd.Parameters.AddWithValue("@password", usuario.Password);
            cmd.Parameters.AddWithValue("@email", usuario.Email);

            cmd.ExecuteNonQuery(); // Ejecuta el INSERT
            conexion.Close();
        }

        internal void DeleteUsuario(int id)
        {
            string sql = "DELETE FROM usuarios WHERE iduser = @id";

            using var conexion = new SqlConnection(_conexionString);
            conexion.Open();

            using var cmd = new SqlCommand(sql, conexion);
            cmd.Parameters.AddWithValue("@id", id);

            cmd.ExecuteNonQuery();
            conexion.Close();

        }

        internal void EditUsuario(UserModel usuario)
        {
            string sql = @"UPDATE usuarios 
                   SET username = @username, 
                       password = @password, 
                       email = @email 
                   WHERE iduser = @id";

            using var conexion = new SqlConnection(_conexionString);
            conexion.Open();

            using var cmd = new SqlCommand(sql, conexion);
            cmd.Parameters.AddWithValue("@username", usuario.Username);
            cmd.Parameters.AddWithValue("@password", usuario.Password);
            cmd.Parameters.AddWithValue("@email", usuario.Email);
            cmd.Parameters.AddWithValue("@id", usuario.Id);

            cmd.ExecuteNonQuery();
            conexion.Close();

        }
    }
}
