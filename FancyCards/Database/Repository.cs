using FancyCards.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace FancyCards.Database
{
    public class Repository
    {
        private readonly AppDbContext _context;

        public Repository(AppDbContext appDbContext)
        {
            _context = appDbContext;
        }

        public async Task<Deck> GetDeckByIdAsync(int id)
        {
            return await _context.Set<Deck>()
                .Include(d => d.Cards)
                    .ThenInclude(c => c.Scores)
                .Include(d => d.Cards)
                    .ThenInclude(c => c.Audio)
                .FirstOrDefaultAsync(d => d.Id == id);
        }

        public async Task<IEnumerable<Deck>> GetAllDecksAsync()
        {
            return await _context.Set<Deck>()
                .Include(d => d.Cards)
                //    .ThenInclude(c => c.Scores)
                //.Include(d => d.Cards)
                //    .ThenInclude(c => c.Audio)
                .ToListAsync();
        }

        public async Task AddCardToDeckAsync(int deckId, Card card)
        {
            var deck = await _context.Decks.FirstOrDefaultAsync(d => d.Id == deckId);
            if (deck != null)
            {
                deck.Cards.Add(card);
                await _context.SaveChangesAsync();
            }
        }

        public async Task<bool> RemoveCardFromDeckAsync(int deckId, int cardId)
        {
            try
            {
                var deck = await _context.Decks.FirstOrDefaultAsync(d => d.Id == deckId);
                var card = deck?.Cards.FirstOrDefault(c => c.Id == cardId);
                if (card != null)
                {
                    deck.Cards.Remove(card);
                    await _context.SaveChangesAsync();
                }
                return true;
            }
            catch(Exception ex)
            {
                Debug.WriteLine(ex.Message);
                return false;
            }

        }


        public async Task RemoveDeckAsync(int id)
        {
            var entity = await _context.Decks.FirstOrDefaultAsync(d => d.Id == id);
            if (entity != null)
                _context.Decks.Remove(entity);

            await _context.SaveChangesAsync();
        }

        public async Task SaveChangesAsync() => await _context.SaveChangesAsync();

        #region obsolete
        //public T Get<T>(int id) where T : EntityBase
        //{
        //    return _context.Set<T>().FirstOrDefault(o => o.Id == id);
        //}

        //public ICollection<T> GetAll<T>() where T : EntityBase
        //{
        //    return _context.Set<T>().ToList() ?? new List<T>();
        //}

        //public async Task<T> Save<T>(T entity, bool saveChanges = true) where T : EntityBase, new()
        //{
        //    try
        //    {
        //        var ent = _context.Find<T>(entity.Id);

        //        if (ent is null)
        //        {
        //            _context.Set<T>().Add(entity);
        //        }
        //        else
        //        {
        //            //this.UpdateValues(ent, entity);
        //            _context.Set<T>().Update(ent);
        //        }

        //        if (saveChanges) await _context.SaveChangesAsync();

        //        return entity;

        //    }
        //    catch (Exception e)
        //    {
        //        Debug.WriteLine(e.Message);
        //        return null;
        //    }
        //}

        //public async Task Save<T>(IEnumerable<T> entities, bool saveChanges = true) where T : EntityBase
        //{
        //    try
        //    {
        //        foreach (var entity in entities)
        //        {
        //            var ent = _context.Find<T>(entity.Id);
        //            if (ent is null)
        //            {
        //                _context.Set<T>().Add(entity);
        //            }
        //            else
        //            {
        //                //_context.Set<T>().Entry(ent).CurrentValues.SetValues(entity);
        //                //this.UpdateValues(ent, entity);
        //                _context.Set<T>().Update(ent);
        //            }

        //        }
        //        if (saveChanges) await _context.SaveChangesAsync();
        //    }
        //    catch (Exception e)
        //    {
        //        Debug.WriteLine(e.Message);
        //    }
        //}

        //public async Task Remove<T>(T entity, bool saveChanges = true) where T : EntityBase, new()
        //{
        //    try
        //    {
        //        _context.Set<T>().Remove(entity);

        //        if (saveChanges) await _context.SaveChangesAsync();
        //    }
        //    catch (Exception e)
        //    {
        //        Debug.WriteLine(e.Message);
        //    }

        //}

        //public async Task Remove<T>(IEnumerable<T> entities, bool saveChanges = true) where T : EntityBase, new()
        //{
        //    try
        //    {
        //        foreach (var entity in entities)
        //        {
        //            _context.Set<T>().Remove(entity);
        //        }

        //        if (saveChanges) await _context.SaveChangesAsync();
        //    }
        //    catch (Exception e)
        //    {
        //        Debug.WriteLine(e.Message);
        //    }
        //}

        //public async Task SaveChangesAsync()
        //{
        //    await _context.SaveChangesAsync();
        //}

        //private void UpdateValues<T>(T entity, T changedEntity) where T : EntityBase
        //{
        //    var entry = _context.Entry(entity);

        //    entry.CurrentValues.SetValues(changedEntity);

        //    var entityType = entry.Metadata;
        //    //обновляем только owned properties
        //    //TODO передалать чтобы обновлялось рекурсивно
        //    foreach (var navigation in entityType.GetNavigations())
        //    {
        //        if (navigation.IsOnDependent || navigation.IsCollection || !navigation.ForeignKey.IsOwnership)
        //        {
        //            continue;
        //        }

        //        entry.Reference(navigation.Name).TargetEntry.CurrentValues.SetValues(navigation.GetGetter().GetClrValue(changedEntity));
        //    }
        //}
        #endregion
    }
}
