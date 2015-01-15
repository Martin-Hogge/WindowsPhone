using System;
using System.Collections.Generic;
using System.Text;
using Windows.Storage;
using Newtonsoft.Json;
using ApplicationMobile_WP.Model;
using Windows.Storage.Streams;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using System.IO;
using ApplicationMobile_WP.Exceptions;
using ApplicationMobile_WP.ViewModel;

namespace ApplicationMobile_WP.DataAccess
{
    class LocalDataAccessManager
    {
        public const string favoriteFileName = "favorites.json";
        public const string recentSearchesFileName = "recentSearches.json";

        public async static Task<bool> AddRecentResearch(Summoner summoner)
        {
            var localFolder = Windows.Storage.ApplicationData.Current.LocalFolder;
            List<Summoner> list = await GetListSummonersFromLocal(recentSearchesFileName);
            StorageFile storageFile = await localFolder.CreateFileAsync(recentSearchesFileName, CreationCollisionOption.ReplaceExisting);
            if (!AlreadyExistsInList(list, summoner))
            {
                if (list.Count < 10)
                {
                    list.Add(summoner);
                }
                else
                {
                    list.RemoveAt(0);
                    list.Add(summoner);
                }
            }
            else
            {
                list.RemoveAll(x => x.ID == summoner.ID && x.Region == summoner.Region);
                list.Add(summoner);
            }
            await UpdateRecentSearches(list, storageFile);
            return true;
        }

        private static async Task UpdateRecentSearches(List<Summoner> list, StorageFile storageFile)
        {
            string jsonText = JsonConvert.SerializeObject(list);

            using (IRandomAccessStream textStream = await storageFile.OpenAsync(FileAccessMode.ReadWrite))
            {
                using (DataWriter textWriter = new DataWriter(textStream))
                {
                    textWriter.WriteString(jsonText);
                    await textWriter.StoreAsync();
                }
            }
            list.Reverse();
            MainViewModel.RecentSearches = new System.Collections.ObjectModel.ObservableCollection<Summoner>(list);
        }

        public async static Task<bool> AddFavorite(Summoner summoner)
        {
            var localFolder = Windows.Storage.ApplicationData.Current.LocalFolder;
            List<Summoner> list = await GetListSummonersFromLocal(favoriteFileName);
            StorageFile storageFile = await localFolder.CreateFileAsync(favoriteFileName, CreationCollisionOption.ReplaceExisting);
            if (AlreadyExistsInList(list, summoner))
            {
                await UpdateFavoriteList(list, storageFile);
                throw new LocalStorageException(LocalStorageException.exceptionType.ALREADY_EXISTS);
            }
            if (list.Count >= 10)
            {
                await UpdateFavoriteList(list, storageFile);
                throw new LocalStorageException(LocalStorageException.exceptionType.TOO_MUCH);
            }

            list.Add(summoner);
            await UpdateFavoriteList(list, storageFile);
            return true;
        }

        private static async Task UpdateFavoriteList(List<Summoner> list, StorageFile storageFile)
        {
            string jsonText = JsonConvert.SerializeObject(list);

            using (IRandomAccessStream textStream = await storageFile.OpenAsync(FileAccessMode.ReadWrite))
            {
                using (DataWriter textWriter = new DataWriter(textStream))
                {
                    textWriter.WriteString(jsonText);
                    await textWriter.StoreAsync();
                }
            }

            MainViewModel.Favoris = new System.Collections.ObjectModel.ObservableCollection<Summoner>(list);
        }

        public async static Task<List<Summoner>> GetListSummonersFromLocal(string fileName)
        {
            var localFolder = Windows.Storage.ApplicationData.Current.LocalFolder;
            try
            {
                StorageFile storageFile = await localFolder.GetFileAsync(fileName);
                using (IRandomAccessStream textStream = await storageFile.OpenReadAsync())
                {
                    using (DataReader textReader = new DataReader(textStream))
                    {
                        uint textLength = (uint)textStream.Size;
                        if (textLength == 0)
                            return new List<Summoner>();
                        await textReader.LoadAsync(textLength);
                        string jsonContents = textReader.ReadString(textLength);
                        List<Summoner> list = JArray.Parse(jsonContents).ToObject<List<Summoner>>();
                        return list;
                    }
                }
            }
            catch (FileNotFoundException)
            {
                return new List<Summoner>();
            }
        }

        public async static Task<bool> RemoveFromFavorites(Summoner summoner)
        {
            var localFolder = Windows.Storage.ApplicationData.Current.LocalFolder;
            List<Summoner> list = await GetListSummonersFromLocal(favoriteFileName);
            StorageFile storageFile = await localFolder.CreateFileAsync(favoriteFileName, CreationCollisionOption.ReplaceExisting);
            list.RemoveAll(x => x.ID == summoner.ID && x.Region == summoner.Region);
            await UpdateFavoriteList(list, storageFile);
            return true;
        }

        public static bool AlreadyExistsInList(List<Summoner> list, Summoner summoner)
        {
            foreach (Summoner s in list)
            {
                if (s.ID == summoner.ID && s.Region == summoner.Region)
                    return true;
            }
            return false;
        }

        //private async static Task<List<KeyValuePair<int, string>>> GetMatchToInsertFromLocal ()
        //{
        //    var localFolder = Windows.Storage.ApplicationData.Current.LocalFolder;
        //    try
        //    {
        //        StorageFile storageFile = await localFolder.GetFileAsync(matchToInsertFileName);
        //        using (IRandomAccessStream textStream = await storageFile.OpenReadAsync())
        //        {
        //            using (DataReader textReader = new DataReader(textStream))
        //            {
        //                uint textLength = (uint)textStream.Size;
        //                if (textLength == 0)
        //                    return new List<KeyValuePair<int, string>>();
        //                await textReader.LoadAsync(textLength);
        //                string jsonContents = textReader.ReadString(textLength);
        //                List<KeyValuePair<int, string>> list = JArray.Parse(jsonContents).ToObject<List<KeyValuePair<int, string>>>();
        //                return list;
        //            }
        //        }
        //    }
        //    catch (FileNotFoundException)
        //    {
        //        return new List<KeyValuePair<int,string>>();
        //    }
        //}

        //public async static Task<bool> AddListOfMatchToInsert(List<KeyValuePair<int, string>> listToAdd)
        //{
        //    var localFolder = Windows.Storage.ApplicationData.Current.LocalFolder;
        //    List<KeyValuePair<int, string>> localList = await GetMatchToInsertFromLocal();
        //    StorageFile storageFile = await localFolder.CreateFileAsync(matchToInsertFileName, CreationCollisionOption.ReplaceExisting);
        //    foreach (KeyValuePair<int, string> entry in listToAdd)
        //    {
        //        bool exist = false;
        //        foreach (KeyValuePair<int, string> localEntry in localList)
        //        {
        //            if (entry.Key == localEntry.Key && entry.Value == localEntry.Value)
        //                exist = true;
        //        }
        //        if (!exist)
        //            localList.Add(entry);
        //    }
        //    string jsonText = JsonConvert.SerializeObject(localList);

        //    using (IRandomAccessStream textStream = await storageFile.OpenAsync(FileAccessMode.ReadWrite))
        //    {
        //        using (DataWriter textWriter = new DataWriter(textStream))
        //        {
        //            textWriter.WriteString(jsonText);
        //            await textWriter.StoreAsync();
        //        }
        //    }
        //    return true;
        //}
    }
}
