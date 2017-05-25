using UnityEngine.SceneManagement;

namespace UnityEngine.ResourceManagement
{
    public class ResourceManager
    {
        static private IResourceManager _instance = null;
        static public IResourceManager instance
        {
            get 
            {
                if (_instance == null)
                    _instance = new ResourceManagerImpl();
                return _instance;
            }
            set
            {
                if (_instance != null)
                    _instance.Shutdown();
                _instance = value;
            }
        }

        static public void AddResourceLocator(IResourceLocator locator, bool prepend)
        {
            instance.AddResourceLocator(locator, prepend);
        }

        static public void AddResourceProvider(IResourceProvider provider)
        {
            instance.AddResourceProvider(provider);
        }

        static public IResourceLocation GetLocation(string id)
        {
            return instance.GetLocation(id);
        }

        static public IResourceProvider GetProvider<T>(IResourceLocation loc) where T : Object
        {
            return instance.GetProvider<T>(loc);
        }

        static public void Release<T>(string id) where T : Object
        {
            instance.Release<T>(id);
        }

        static public T Load<T>(string id) where T : Object
        {
            return instance.Load<T>(id);
        }

        static public IAsyncOperation<T> LoadAsync<T>(string id) where T : Object
        {
            return instance.LoadAsync<T>(id);
        }

        static public T Instantiate<T>(string id) where T : Object
        {
            return instance.Instantiate<T>(id);
        }

        static public IAsyncOperation<T> InstantiateAsync<T>(string id) where T : Object
        {
            return instance.InstantiateAsync<T>(id);
        }

        static public void ReleaseInstance<T>(string id, T inst) where T : Object
        {
            instance.ReleaseInstance<T>(id, inst);
        }

        static public SceneManagement.Scene LoadScene(string id, LoadSceneMode mode)
        {
            return instance.LoadScene(id, mode);
        }

        static public IAsyncOperation<SceneManagement.Scene> LoadSceneAsync(string id, LoadSceneMode mode)
        {
            return instance.LoadSceneAsync(id, mode);
        }

    }
}