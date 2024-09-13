using Invasion.Math;
using System;
using System.Collections.Generic;

namespace Invasion.ECS
{
    public class GameObject
    {
        public string Name { get; set; } = string.Empty;

        public Transform Transform => GetComponent<Transform>();

        private GameObject? Parent { get; set; }

        private Dictionary<Type, Component> Components { get; } = [];
        private Dictionary<string, GameObject> Children { get; } = [];

        private GameObject() { }

        public void AddComponent<T>(T component) where T : Component
        {
            component.GameObject = this;
            component.Initialize();
            Components.Add(typeof(T), component);
        }

        public T GetComponent<T>() where T : Component
        {
            return (T)Components[typeof(T)];
        }

        public void RemoveComponent<T>() where T : Component
        {
            Components[typeof(T)].CleanUp();
            Components.Remove(typeof(T));
        }

        public void AddChild(GameObject child)
        {
            child.Parent = this;
            child.GetComponent<Transform>().Parent = GetComponent<Transform>();

            Children.Add(child.Name, child);
        }

        public GameObject GetChild(string name)
        {
            return Children[name];
        }

        public void RemoveChild(string name)
        {
            Children[name].Parent = null;
            Children[name].GetComponent<Transform>().Parent = null;

            Children.Remove(name);
        }

        public GameObject? GetParent()
        {
            return Parent;
        }

        public void Update()
        {
            foreach (Component component in Components.Values)
                component.Update();
        }

        public void Render()
        {
            foreach (Component component in Components.Values)
                component.Render();
        }

        public void CleanUp()
        {
            foreach (Component component in Components.Values)
                component.CleanUp();
        }

        public static GameObject Create(string name)
        {
            GameObject result = new()
            {
                Name = name 
            };

            result.AddComponent(Transform.Create());

            GameObjectManager.Register(result);

            return result;
        }
    }
}
