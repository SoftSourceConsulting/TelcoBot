/*  
 *  ISimpleRepository.cs
 *  Neil McKamey-Gonzalez
 *  Softsource Consulting, Inc.
 */

using System.Collections.Generic;
using TelcoBot.Model;

namespace TelcoBot.Persistence
{
    public interface ISimpleRepository<T> : IReadOnlyCollection<T>
        where T : IIdentified
    {
        void Save(T item);
        T FindById(int id);
    }
}