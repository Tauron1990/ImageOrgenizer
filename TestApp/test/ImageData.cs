using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using JetBrains.Annotations;

namespace ImageOrganizer.BL
{
    public class ImageData : INotifyPropertyChanged
    {
        private bool _favorite;

        public ImageData(string name, string providerName, int randomCount, int viewCount, DateTime added, ICollection<TagData> tags, int id, string author, bool favorite)
        {
            Name = name;
            ProviderName = providerName;
            RandomCount = randomCount;
            ViewCount = viewCount;
            Added = added;
            Tags = tags;
            Id = id;
            Author = author;
            Favorite = favorite;
        }

        public ImageData(string name, string providerName)
        {
            Name = name;
            ProviderName = providerName;
            New = true;
        }

        public bool New { get; }

        public bool Favorite
        {
            get => _favorite;
            set
            {
                _favorite = value;
                OnPropertyChanged();
            }
        }

        public int Id { get; }

        public string Author { get; set; }

        public string Name { get; set; }

        public string ProviderName { get; }

        public int RandomCount { get; }

        public int ViewCount { get; }

        public ICollection<TagData> Tags { get; }

        public DateTime Added { get; set; }

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        private void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public override string ToString() => $"{Name}-{ProviderName}-{ViewCount}";
    }
}