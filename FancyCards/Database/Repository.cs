using FancyCards.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Text;
using System.Windows.Controls;

namespace FancyCards.Database
{
    public class Repository
    {
        private readonly AppDbContext _context;

        public Repository(AppDbContext appDbContext)
        {
            _context = appDbContext;
        }


        public async Task<Deck> GetDeckAsync(int id)
        {
            return await _context.Set<Deck>()
                .Include(d => d.Cards)
                .Include(d => d.Settings.ReviewProfile)
                .FirstOrDefaultAsync(d => d.Id == id);
        }

        public async Task<IEnumerable<Deck>> GetAllDecksAsync()
        {
            return await _context.Set<Deck>()
                .Include(d => d.Cards)
                .Include(d => d.Settings.ReviewProfile)
                .ToListAsync();
        }

        public async Task<IEnumerable<TrainingSession>> GetAllTrainingSessionsAsync()
        {
            return await _context.Set<TrainingSession>()
                .Include(t => t.Cards)
                .ToListAsync();
        }

        //---------------------------------------------------------------------------------------------------------------------------------------------------------------

        public Task<T> GetAsync<T>(int id) where T : EntityBase => _context.Set<T>().FirstOrDefaultAsync(o => o.Id == id);
        public async Task<IEnumerable<T>> GetAllAsync<T>() where T : EntityBase => await _context.Set<T>().ToListAsync();


        public async Task<bool> AddAsync<T>(T entity) where T : EntityBase
        {
            try
            {
                _context.Set<T>().Add(entity);
                await _context.SaveChangesAsync();

                return true;
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
                return false;
            }
        }

        public async Task<bool> UpdateAsync<T>(T entity) where T : EntityBase
        {
            try
            {
                var db_entity = _context.Set<T>().FirstOrDefault(r => r.Id == entity.Id);
                if (db_entity != null)
                {
                    UpdateValues(db_entity, entity);
                }

                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
                return false;
            }
        }

        public async Task<bool> AddOrUpdateAsync<T>(IEnumerable<T> entities) where T : EntityBase
        {
            try
            {
                foreach(var entity in entities)
                {
                    var db_entity = _context.Set<T>().FirstOrDefault(r => r.Id == entity.Id);
                    if (db_entity == null)
                    {
                        _context.Set<T>().Add(entity);
                    }
                    else
                    {
                        UpdateValues(db_entity, entity);
                    }
                }

                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
                return false;
            }
        }

        public async Task<bool> RemoveAsync<T>(T entity) where T : EntityBase
        {
            try
            {
                var db_entity = _context.Set<T>().FirstOrDefault(r => r.Id == entity.Id);
                _context.Set<T>().Remove(db_entity);
                await _context.SaveChangesAsync();

                return true;
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
                return false;
            }
        }

        //-------------------------------------------------------------------------------------------------------------------------------------------------------------

        public async Task SaveChangesAsync() => await _context.SaveChangesAsync();

        public void UpdateValues<T>(T entity, T changedEntity) where T : EntityBase
        {
            var entry = _context.Entry(entity);

            entry.CurrentValues.SetValues(changedEntity);

            var entityType = entry.Metadata;
            //обновляем только owned properties
            //TODO передалать чтобы обновлялось рекурсивно
            foreach (var navigation in entityType.GetNavigations())
            {
                if (navigation.IsOnDependent || navigation.IsCollection || !navigation.ForeignKey.IsOwnership)
                {
                    continue;
                }

                entry.Reference(navigation.Name).TargetEntry.CurrentValues.SetValues(navigation.GetGetter().GetClrValue(changedEntity));
            }
        }



    }
}
