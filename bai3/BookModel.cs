using System;
using System.ComponentModel;
using System.IO;

namespace BookManager
{
    public class BookModel : INotifyPropertyChanged
    {
        private string bookId = "";
        private string title = "";
        private string author = "";
        private string category = "";
        private int quantity;
        private decimal unitPrice;
        private string coverPath = "";

        public string BookId
        {
            get => bookId;
            set { bookId = value; OnPropertyChanged(nameof(BookId)); }
        }

        public string Title
        {
            get => title;
            set { title = value; OnPropertyChanged(nameof(Title)); }
        }

        public string Author
        {
            get => author;
            set { author = value; OnPropertyChanged(nameof(Author)); }
        }

        public string Category
        {
            get => category;
            set { category = value; OnPropertyChanged(nameof(Category)); }
        }

        public int Quantity
        {
            get => quantity;
            set { quantity = value; OnPropertyChanged(nameof(Quantity)); }
        }

        public decimal UnitPrice
        {
            get => unitPrice;
            set { unitPrice = value; OnPropertyChanged(nameof(UnitPrice)); }
        }

        public string CoverPath
        {
            get => coverPath;
            set { coverPath = value; OnPropertyChanged(nameof(CoverPath)); }
        }

        public decimal TotalValue => Quantity * UnitPrice;

        public event PropertyChangedEventHandler? PropertyChanged;

        private void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(
                this,
                new PropertyChangedEventArgs(propertyName));

            if (propertyName == nameof(Quantity) ||
                propertyName == nameof(UnitPrice))
            {
                PropertyChanged?.Invoke(
                    this,
                    new PropertyChangedEventArgs(nameof(TotalValue)));
            }
        }
    }
}
