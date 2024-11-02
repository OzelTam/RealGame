using RealGame.GameEngine.Entities.Interfaces;

namespace RealGame.GameEngine.Entities
{
    public class ResourceRepository<T> where T : class, IIdentifiable
    {
        private Dictionary<string, T> resources = new Dictionary<string, T>();
        private Dictionary<string, HashSet<string>> TagIdMap = new Dictionary<string, HashSet<string>>();


        public T? this[string id]
        {
            get
            {
                if (resources.ContainsKey(id))
                {
                    return resources[id];
                }
                return null;
            }
            set
            {
                if (value == null)
                {
                    Remove(id);
                }
                else if (resources.ContainsKey(id))
                {
                    resources[id] = value;
                }
                else
                {
                    resources.Add(id, value);
                }
            }
        }

        public void Add(T value)
        {
            var id = value.Id;
            if (resources.ContainsKey(id))
                resources[id] = value;
            else
                resources.Add(id, value);

            if (!TagIdMap.ContainsKey(value.Tag))
            {
                TagIdMap.Add(value.Tag, new HashSet<string>());
            }

            TagIdMap[value.Tag].Add(value.Id);
        }

        public bool Contains(string id)
        {
            return resources.ContainsKey(id);
        }
        public bool Contains(T value)
        {
            return resources.ContainsKey(value.Id);
        }

        public bool ContainsTag(string tag)
        {
            return TagIdMap.ContainsKey(tag);
        }

        public List<T> GetAll(string tag)
        {
            if (TagIdMap.ContainsKey(tag))
            {
                return TagIdMap[tag].Select(id => resources[id]).ToList();
            }
            return new List<T>();
        }

        public IEnumerable<T> GetAll(Func<T, bool>? predicate = null)
        {
            foreach (var value in resources.Values)
            {
                if (predicate != null)
                {
                    if (predicate(value))
                    {
                        yield return value;
                    }
                }
                else
                {
                    yield return value;
                }
            }
        }

        public T? Get(string id)
        {
            return this[id];
        }

        public void Remove(string id)
        {
            if (resources.ContainsKey(id))
            {
                var value = resources[id];
                resources.Remove(id);
                TagIdMap[value.Tag].Remove(id);
            }
        }

        public void Remove(T value)
        {
            Remove(value.Id);
        }

        public void RemoveAll(Func<T, bool> predicate)
        {
            var ids = resources.Keys.Where(k => predicate(resources[k])).ToList();
            foreach (var id in ids)
            {
                Remove(id);
            }
        }

        public void RemoveAll(string tag)
        {
            if (TagIdMap.ContainsKey(tag))
            {
                var ids = TagIdMap[tag].ToList();
                foreach (var id in ids)
                {
                    Remove(id);
                }
            }
        }

        public void Clear()
        {
            resources.Clear();
            TagIdMap.Clear();
        }

        public IEnumerable<string> Ids => resources.Keys;
        public IEnumerable<T> Resources => resources.Values;
        public IEnumerable<string> Tags => TagIdMap.Keys;
        public int Count => resources.Count;
        public int TagCount => TagIdMap.Count;



    }
}
