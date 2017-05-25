using System;
using System.Collections.Generic;
using UnityEngine.Networking;
using System.Linq;

namespace UnityEngine.ResourceManagement
{
    public class ResourceManagerImpl : IResourceManager
    {
        List<IResourceLocator> m_locators = new List<IResourceLocator>();
        List<IResourceProvider> m_providers = new List<IResourceProvider>();
        IResourceProvider instanceProvider;
        SceneProvider sceneProvider;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        static void Init()
        {
            ResourceManager.instance = new ResourceManagerImpl();
            ResourceManager.instance.AddResourceLocator(Resources.Load<ContentCatalog>("ContentCatalog"), false);
            ResourceManager.instance.AddResourceLocator(new DefaultLocator(), false);
            ResourceManager.instance.AddResourceProvider(new CachedProvider("Asset Bundles", new AssetBundleProvider(new LocalAssetBundleProvider())));
            ResourceManager.instance.AddResourceProvider(new CachedProvider("Assets", new BundledAssetProvider()));
            ResourceManager.instance.AddResourceProvider(new LocalAssetProvider());
            InitAssetVariants();
        }

        public ResourceManagerImpl()
        {
            sceneProvider = new SceneProvider();
            instanceProvider = new InstanceProvider();
        }

        public void Shutdown()
        {
        }

        static void InitAssetVariants()
        {
            ResourceManager.instance.LoadAsync<AssetVariantMappingObject>("assets/assetvariantmaps.asset").complete += (obj) => {
                if(obj.result)
                {
                    AssetVariantMapping.SetAssetVariantMaps(obj.result.assetVariantMaps);
                }
                else
                {
                    Debug.LogWarning("Could not load asset variant mapping bundle!");
                }
            };
        }

        //this allows for custom locators to be injected into the system.  Usually used in conjunction
        //with a custom provider that understands the locations passed to it by the locator
        public void AddResourceLocator(IResourceLocator locator, bool prepend)
        {
            if (locator == null)
                return;

            if (prepend)
                m_locators.Insert(0, locator);
            else
                m_locators.Add(locator);
            locator.OnAddedToResourceManager();
        }

        public void AddResourceProvider(IResourceProvider provider)
        {
            m_providers.Add(provider);
        }

        public void Release<T>(string id) where T : Object
        {
            var loc = GetLocation(id);
            GetProvider<T>(loc).Release<T>(loc, null);
        }

        public T Load<T>(string id) where T : Object
        {
            var loc = GetLocation(id);
            return GetProvider<T>(loc).Provide<T>(loc);
        }

        public IAsyncOperation<T> LoadAsync<T>(string id) where T : Object
        {
            var loc = GetLocation(id);
            return GetProvider<T>(loc).ProvideAsync<T>(loc);
        }

        public T Instantiate<T>(string id) where T : Object
        {
            return instanceProvider.Provide<T>(GetLocation(id));
        }

        public IAsyncOperation<T> InstantiateAsync<T>(string id) where T : Object
        {
            return instanceProvider.ProvideAsync<T>(GetLocation(id));
        }

        public void ReleaseInstance<T>(string id, T inst) where T : Object
        {
            var loc = GetLocation(id);
            instanceProvider.Release<T>(loc, inst);
        }

        public SceneManagement.Scene LoadScene(string id, SceneManagement.LoadSceneMode mode)
        {
            return sceneProvider.Provide(GetLocation(string.Concat("Scene::", id)), mode);
        }

        public IAsyncOperation<SceneManagement.Scene> LoadSceneAsync(string id, SceneManagement.LoadSceneMode mode)
        {
            return sceneProvider.ProvideAsync(GetLocation(string.Concat("Scene::", id)), mode);
        }

        public abstract class ResourceProviderBase : IResourceProvider
        {
            protected GenericObjectCache cache = new GenericObjectCache();
            public abstract bool CanProvide<T>(IResourceLocation loc) where T : Object;
            public abstract T Provide<T>(IResourceLocation loc) where T : Object;
            public abstract IAsyncOperation<T> ProvideAsync<T>(IResourceLocation loc) where T : Object;
            public abstract bool Release<T>(IResourceLocation loc, T asset) where T : Object;
        }

        public IResourceLocation GetLocation(string id)
        {
            for (int i = 0; i < m_locators.Count; i++)
            {
                var l = m_locators[i].Locate(id);
                if (l != null)
                    return l;
            }
            return null;
        }

        public IResourceProvider GetProvider<T>(IResourceLocation loc) where T : Object
        {
            for (int i = 0; i < m_providers.Count; i++)
            {
                var p = m_providers[i];
                if (p.CanProvide<T>(loc))
                    return p;
            }
            return null;
        }

        public class DefaultLocator : IResourceLocator
        {
            public IResourceLocation Locate(string id)
            {
                return new ResourceLocation(id, id, "localasset");
            }

            public void OnAddedToResourceManager()
            {
            }
        }

        [System.Serializable]
        public class ResourceLocation : IResourceLocation, ISerializationCallbackReceiver, IComparable
        {
            [SerializeField]
            string m_name;
            [SerializeField]
            string m_id;
            [SerializeField]
            string m_method;
            [SerializeField]
            string[] m_dependencies;
            int m_hashCode;
            int m_methodHashCode;
            public string name { get { return m_name; } } 
            public string id { get { return m_id; } }     
            public int method { get { return m_methodHashCode; } } 
            public string[] dependencies { get { return m_dependencies; } set { m_dependencies = value; } }
            public ResourceLocation() { }
            public ResourceLocation(string name, string id, string method, params string[] dependencies)
            {
                m_name = name;
                m_id = id;
                m_method = method;
                m_dependencies = dependencies;
                OnAfterDeserialize();
            }

            public override string ToString()
            {
                return m_name + "->" + m_id + "[" + m_method + "]";
            }

            public override int GetHashCode()
            {
                return m_hashCode;
            }

            public void OnBeforeSerialize()
            {
            }

            public void OnAfterDeserialize()
            {
                m_methodHashCode = m_method.GetHashCode();
                m_hashCode = (m_name + m_id).GetHashCode() + m_methodHashCode;
            }

            public int CompareTo(object obj)
            {
                return m_name.CompareTo((obj as IResourceLocation).name);
            }

        }
    }
}