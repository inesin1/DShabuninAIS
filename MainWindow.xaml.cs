using DShabuninAIS.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace DShabuninAIS
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private is32ShabuninContext _dbContext = new is32ShabuninContext(); // Контекст БД
        private string _currentTable; // Имя текущей таблицы

        public MainWindow()
        {
            new LoginWindow().ShowDialog(); //Создаем окно авторизации и открываем его

            InitializeComponent();

            //Устанавливаем текущей таблицей "Пользователи"
            _currentTable = "Студенты";
            //Обновляем текущую таблицу
            RefreshTable(_currentTable);
        }

        /// <summary>
        /// Обновляет данные в переданной таблице
        /// </summary>
        /// <param name="tableName">Имя таблицы</param>
        private void RefreshTable(string tableName)
        {
            switch (tableName)
            { //Проверяем пришедшую переменную tableName
                case "Студенты": //В случае если имя таблицы - Пользователи
                    _dbContext.Students.Load(); //Загружаем данные таблицы из БД
                    _dbContext.Groups.Load();
                    //Устанавливаем загруженные данные таблицы из БД как источник для
                    //вывода в DataGrid. Вспоминайте, DataGrid не хранит данные, он их
                    //только выводит
                    studentsDG.ItemsSource = _dbContext.Students.Local.ToObservableCollection();
                    groupsCB.ItemsSource = _dbContext.Groups.Local.Select(g => g.Name).ToList();
                    groupsCB.SelectedIndex = 0;
                    break;
                case "Группы": //В случае если имя таблицы - Пользователи
                    _dbContext.Groups.Load(); //Загружаем данные таблицы из БД
                    //Устанавливаем загруженные данные таблицы из БД как источник для
                    //вывода в DataGrid. Вспоминайте, DataGrid не хранит данные, он их
                    //только выводит
                    groupsDG.ItemsSource = _dbContext.Groups.Local.ToObservableCollection();
                    break;
            }
        }

        private void dataGrid_AutoGeneratingColumn(object sender, DataGridAutoGeneratingColumnEventArgs e)
        {
            //Имя столбца
            string headerName = e.Column.Header.ToString();

            //Проверяем имя столбца
            switch (headerName)
            {
                case "Id":
                    e.Column.Header = "Ид";
                    break;
                case "Name":
                    e.Column.Header = "Имя";
                    break;
                case "Group":
                    e.Column.Visibility = Visibility.Collapsed;
                    break;
                case "Students":
                    e.Column.Visibility = Visibility.Collapsed;
                    break;
                case "GroupId":
                    //Скрываем столбец
                    e.Column.Visibility = Visibility.Collapsed;

                    _dbContext.Groups.Load(); //Подгружаем данные из таблицы Roles

                    Binding binding = new Binding(); //Создаем новый биндинг для подвязки роли
                    binding.Path = new PropertyPath("GroupId"); //В путь подвязки указываем поле RoleId

                    //Создаем новый столбец типа ComboBox для 
                    //возможности выбора роли и настраиваем его
                    DataGridComboBoxColumn col = new DataGridComboBoxColumn
                    {
                        Header = "Группа", //Название столбца
                        DisplayMemberPath = "Name", //Отображаем именно поле Name, а не ID
                        SelectedValuePath = "Id", //А выбираем по ID
                        ItemsSource = _dbContext.Groups.ToArray(), //Подвязываем эти данные в выпадающий список выбора
                        SelectedValueBinding = binding //Устанавливаем созданный ранее биндинг к столбцу
                    };

                    ((DataGrid)sender).Columns.Add(col); //Добавляем созданный столбец в DataGrid
                    break;
            }
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            _dbContext.SaveChanges();
        }

        private void DeleteButton_Click(object sender, RoutedEventArgs e)
        {
            switch (_currentTable)
            {
                case "Студенты":
                    _dbContext.Students.Local.Remove(studentsDG.SelectedItem as Student);
                    break;
                case "Группы":
                    _dbContext.Groups.Local.Remove(studentsDG.SelectedItem as Group);
                    break;
            }
        }

        private void tab_GotFocus(object sender, RoutedEventArgs e)
        {
            _currentTable = ((TabItem)sender).Header.ToString();
            RefreshTable(_currentTable);
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            RefreshTable(_currentTable);
        }

        private void AddButton_Click(object sender, RoutedEventArgs e)
        {

        }

        private void ExportButton_Click(object sender, RoutedEventArgs e)
        {
            //Получаем путь к файлу для экспорта
            string filePath = GetUserFile();

            //Если вернулся null (Ничего не выбрали), выходим из метода 
            if (filePath == null)
                return;

            //Открываем поток на запись
            StreamWriter file = new StreamWriter(filePath, false);

            //Проверяем какую таблицу будем экспортировать
            switch (_currentTable)
            {
                case "Студенты":
                    //Сохраняем таблицу в коллекцию table для удобства
                    ObservableCollection<Student> table = _dbContext.Students.Local.ToObservableCollection();

                    file.WriteLine($"Ид;Имя;ГруппаИд"); //Записываем заголовки
                    //Проходим по всем элементам таблицы
                    foreach (Student elem in table)
                    {
                        //Записываем каждое поле элемента в файл
                        file.WriteLine($"{elem.Id};{elem.Name};{elem.GroupId}");
                    }
                    break;
                case "Группы":
                    //Сохраняем таблицу в коллекцию table для удобства
                    ObservableCollection<Group> table1 = _dbContext.Groups.Local.ToObservableCollection();

                    file.WriteLine($"Ид;Имя"); //Записываем заголовки
                    //Проходим по всем элементам таблицы
                    foreach (Group elem in table1)
                    {
                        //Записываем каждое поле элемента в файл
                        file.WriteLine($"{elem.Id};{elem.Name}");
                    }
                    break;
            }

            file.Close(); //Закрываем файл
            MessageBox.Show("Экспорт успешно завершен", "Успешно!");
        }

        /// <summary>
        /// Возвращает полный путь к файлу, выбранному пользователем
        /// </summary>
        /// <returns>Полный путь к файлу</returns>
        private string GetUserFile()
        {
            //Создаем OpenFileDialog для выбора файла
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.Title = "Выберите файл для экспорта";

            //Открываем его, и если выбрали файл, то возвращаем путь до него
            if (ofd.ShowDialog() == true)
            {
                return ofd.FileName;
            }

            return null; //Иначе вернется null
        }
    }
}
