﻿using System.Collections.Generic;

namespace Tauron.Application.ImageOrganizer.Data.Repositories
{
    public interface IOptionRepository
    {
        IEnumerable<(string Name, string Value)> GetAllValues();
        
        void Remove(string name);
        void SetValue(string name, string value);
    }
}