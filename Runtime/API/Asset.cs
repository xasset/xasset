using System;
using UnityEngine;
using Object = UnityEngine.Object;

namespace xasset
{
    /// <summary>
    ///     Unity 中资源的加载 API，Unity 中的资源主要包含 Texture, Material, GameObject, Sprite, AnimationClip, VideoClip,
    ///     RuntimeAnimationController 等类型的资源。
    /// </summary>
    public static class Asset
    {
        /// <summary>
        ///     加载资源同步。
        /// </summary>
        /// <param name="path">资源路径</param>
        /// <param name="type">资源类型</param>
        /// <returns></returns>
        public static AssetRequest Load(string path, Type type)
        {
            var request = LoadAsync(path, type);
            request?.WaitForCompletion();
            return request;
        }

        /// <summary>
        ///     加载资源异步
        /// </summary>
        /// <param name="path">资源路径</param>
        /// <param name="type">资源类型</param>
        /// <returns></returns>
        public static AssetRequest LoadAsync(string path, Type type)
        {
            return AssetRequest.Load(path, type);
        }

        /// <summary>
        ///     加载全部资源。例如，从 fbx 中加载多个 AnimationClip，从 Texture 中加载多个 Sprite 等。
        /// </summary>
        /// <param name="path">资源路径</param>
        /// <param name="type">资源类型</param>
        /// <returns></returns>
        public static AssetRequest LoadAll(string path, Type type)
        {
            var request = LoadAllAsync(path, type);
            request?.WaitForCompletion();
            return request;
        }

        /// <summary>
        ///     加载全部资源异步。
        /// </summary>
        /// <param name="path"></param>
        /// <param name="type"></param>
        /// <returns></returns>
        public static AssetRequest LoadAllAsync(string path, Type type)
        {
            return AssetRequest.Load(path, type, true);
        }

        /// <summary>
        ///     异步实例化一个预设到当前场景。
        /// </summary>
        /// <param name="path">预设的资源路径</param>
        /// <param name="parent">父节点</param>
        /// <param name="worldPositionStays">是否保留世界坐标</param>
        /// <returns></returns>
        public static InstantiateRequest InstantiateAsync(string path, Transform parent = null,
            bool worldPositionStays = false)
        {
            return InstantiateRequest.InstantiateAsync(path, parent, worldPositionStays);
        }

        /// <summary>
        ///     获取已经加载的资源。
        /// </summary>
        /// <param name="path"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static T Get<T>(string path) where T : Object
        {
            return AssetRequest.Get<T>(path);
        }

        /// <summary>
        ///     获取已经加载的全部资源。
        /// </summary>
        /// <param name="path"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static T[] GetAll<T>(string path) where T : Object
        {
            return AssetRequest.GetAll<T>(path);
        }
    }
}