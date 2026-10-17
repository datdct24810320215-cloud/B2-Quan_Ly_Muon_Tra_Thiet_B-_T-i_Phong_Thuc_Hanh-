using System;
using System.ComponentModel;

namespace B2_Quan_Ly_Muon_Tra_Thiet_Bị_Tại_Phong_Thuc_Hanh_
{
    public class LoanRecord : INotifyPropertyChanged
    {
        private DateTime? _returnDate;

        public string Id { get; set; }
        public string StudentId { get; set; }
        public string StudentName { get; set; }
        public string Device { get; set; }
        public int Quantity { get; set; }
        public DateTime BorrowDate { get; set; }
        public DateTime DueDate { get; set; }
        public DateTime? ReturnDate
        {
            get => _returnDate;
            set
            {
                _returnDate = value;
                OnPropertyChanged(nameof(ReturnDate));
                OnPropertyChanged(nameof(Status));
            }
        }

        public string Status
        {
            get
            {
                if (ReturnDate.HasValue) return "Đã trả";
                if (DueDate.Date < DateTime.Today) return "Quá hạn";
                return "Đang mượn";
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged(string name)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }
}
