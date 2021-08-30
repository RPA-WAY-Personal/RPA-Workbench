using System;
using System.Activities;
using System.Activities.Presentation;
using System.Activities.Presentation.Model;
using System.Activities.Presentation.Services;
using System.Activities.Presentation.Toolbox;
using System.Activities.Statements;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;

namespace CustomControls.CustomToolbox {

	/// <summary>
	/// Provides a repository for available <see cref="ControlData"/> instances.
	/// </summary>
	class ControlDataRepository {

		public static ControlDataRepository Instance = new ControlDataRepository();

		public event EventHandler<CollectionChangeEventArgs> FavoritesChanged;

		private readonly Dictionary<string, ControlData> store = new Dictionary<string, ControlData>();
		private readonly HashSet<string> favorites = new HashSet<string>();
		private static List<string> namespacesToIgnore = new List<string>
			{
				"Microsoft.VisualBasic.Activities",
				"System.Activities.Expressions",
				"System.Activities.Statements",
				"System.ServiceModel.Activities",
				"System.ServiceModel.Activities.Presentation.Factories",
				"System.Activities.Presentation"
			};

		private IDictionary<string, ToolboxCategory> toolboxCategoryMap;
		private IDictionary<ToolboxCategory, IList<string>> loadedToolboxActivities;
		/////////////////////////////////////////////////////////////////////////////////////////////////////
		// OBJECT
		/////////////////////////////////////////////////////////////////////////////////////////////////////

		/// <summary>
		/// Initializes a new instance of the <c>ControlDataRepository</c> class.
		/// </summary>
		private ControlDataRepository() {
			Reset();
		}

		/////////////////////////////////////////////////////////////////////////////////////////////////////
		// NON-PUBLIC PROCEDURES
		/////////////////////////////////////////////////////////////////////////////////////////////////////

		/// <summary>
		/// Adds <c>ControlData</c> to the repository.
		/// </summary>
		/// <param name="controlData">The <c>ControlData</c> to add.</param>
		private void Add(ControlData controlData) {
			store[controlData.FullName] = controlData;
		}

		/// <summary>
		/// Adds a range of <c>ControlData</c> to the repository.
		/// </summary>
		/// <param name="range">The range of <c>ControlData</c> to add.</param>
		private void AddRange(IEnumerable<ControlData> range) {
			foreach (var controlData in range)
				Add(controlData);
		}

		/// <summary>
		/// Creates a new instance of <see cref="ControlData"/> for a control of the given type.
		/// </summary>
		/// <param name="controlType">The <see cref="Type"/> of the control.</param>
		/// <returns>A new instance of <see cref="ControlData"/>.</returns>
		private static ControlData CreateControlData(Type controlType) {
			// Use the last part of the namespace as the category
			string category = controlType.Namespace.Split('.').Last();
			return new ControlData(controlType.FullName, category);
		}

		/////////////////////////////////////////////////////////////////////////////////////////////////////
		// PUBLIC PROCEDURES
		/////////////////////////////////////////////////////////////////////////////////////////////////////

		/// <summary>
		/// Adds the control data as a favorite.
		/// </summary>
		/// <param name="controlData">The control data.</param>
		/// <returns><c>true</c> if the control data was added as a favorite; otherwise <c>false</c> if it was already a favorite and no action was necessary.</returns>
		public bool AddFavorite(ControlData controlData) {
			if (favorites.Add(controlData.FullName)) {
				FavoritesChanged?.Invoke(this, new CollectionChangeEventArgs(CollectionChangeAction.Add, controlData));
				return true;
			}
			return false;
		}

		/// <summary>
		/// Removes the control data as a favorite.
		/// </summary>
		/// <param name="controlData">The control data.</param>
		/// <returns><c>true</c> if the control data was removed as a favorite; otherwise <c>false</c> if it was not a favorite and no action was necessary.</returns>
		public bool RemoveFavorite(ControlData controlData) {
			if (favorites.Remove(controlData.FullName)) {
				FavoritesChanged?.Invoke(this, new CollectionChangeEventArgs(CollectionChangeAction.Remove, controlData));
			}
			return false;
		}

		/// <summary>
		/// Clears all favorites.
		/// </summary>
		public void ClearFavorites() {
			favorites.Clear();
			FavoritesChanged?.Invoke(this, new CollectionChangeEventArgs(CollectionChangeAction.Refresh, null));
		}

		/// <summary>
		/// Gets an enumerable of the data for all controls in the repository.
		/// </summary>
		public IEnumerable<ControlData> Controls {
			get {
				return store.Values;
			}
		}

		/// <summary>
		/// Gets an enumerable of distinct category names for controls in the repository.
		/// </summary>
		public IEnumerable<string> DistinctCategories {
			get {
				return store.Values.Select(x => x.Category).Distinct();
			}
		}

		/// <summary>
		/// Gets an enumerable of all controls in the repository which have been designated as favorites.
		/// </summary>
		public IEnumerable<ControlData> Favorites {
			get {
				// Iterate the names of all controls marked as favorites
				foreach (string fullName in favorites) {
					ControlData controlData = Find(fullName);
					if (controlData != null)
						yield return controlData;
				}
			}
		}

		/// <summary>
		/// Attempts to find data in the repository for the specified full name of the control.
		/// </summary>
		/// <param name="fullName">The full name of the control.</param>
		/// <returns>the instance of <c>ControlData</c> corresponding to the specified full name when found; otherwise <c>null</c> when no match is found.</returns>
		public ControlData Find(string fullName) {
			if (store.TryGetValue(fullName, out var controlData))
				return controlData;
			return null;
		}

		/// <summary>
		/// Gets an enumerable of all controls in the repository for the given <paramref name="category"/>. Category names are not case-sensitive.
		/// </summary>
		/// <param name="category">The name of the category.</param>
		/// <returns>Returns a enumerable of all controls in the repository for the given category.</returns>
		public IEnumerable<ControlData> FindByCategory(string category) {
			return Controls.Where(x => string.Compare(x.Category, category, StringComparison.InvariantCultureIgnoreCase) == 0);
		}

		/// <summary>
		/// Tests if the specified <c>ControlData</c> is designated as a favorite.
		/// </summary>
		/// <param name="controlData">The control data to test.</param>
		/// <returns><c>true</c> if the <c>ControlData</c> is a favorite; otherwise <c>false</c>.</returns>
		public bool IsFavorite(ControlData controlData) {
			return favorites.Contains(controlData.FullName);
		}

		/// <summary>
		/// Resets the repository to the initial state.
		/// </summary>
		public void Reset() {
			ClearFavorites();
			store.Clear();
			WorkflowDesignerIcons.UseWindowsStoreAppStyleIcons();
			this.loadedToolboxActivities = new Dictionary<ToolboxCategory, IList<string>>();
			this.toolboxCategoryMap = new Dictionary<string, ToolboxCategory>();


			this.AddCategoryToToolbox(
               "Transaction",
               new List<Type>
               {
                    typeof(CancellationScope),
               });
            this.AddRange(new ControlData[] {
				//CreateControlData(typeof(ActiproSoftware.Windows.Controls.Docking.AdvancedTabControl)),
				//CreateControlData(typeof(ActiproSoftware.Windows.Controls.Docking.DockSite)),
				//CreateControlData(typeof(ActiproSoftware.Windows.Controls.Docking.WindowControl)),
				//CreateControlData(typeof(ActiproSoftware.Windows.Controls.Editors.AutoCompleteBox)),
				CreateControlData(typeof(While)),
				CreateControlData(typeof(CancellationScope))

			});
		}

		private bool IsValidToolboxActivity(Type activityType)
		{
			return activityType.IsPublic && !activityType.IsNested && !activityType.IsAbstract
				&& (typeof(Activity).IsAssignableFrom(activityType) || typeof(IActivityTemplateFactory).IsAssignableFrom(activityType) || typeof(FlowNode).IsAssignableFrom(activityType));
		}

		private void AddCategoryToToolbox(List<Assembly> assemblies)
		{
			foreach (Assembly assembly in assemblies)
			{
				foreach (Type activityType in assembly.GetTypes())
				{
					if (this.IsValidToolboxActivity(activityType))
					{
						ToolboxCategory category = this.GetToolboxCategory(activityType.Namespace);

						if (!this.loadedToolboxActivities[category].Contains(activityType.FullName))
						{
							this.loadedToolboxActivities[category].Add(activityType.FullName);
							category.Add(new ToolboxItemWrapper(activityType.FullName, activityType.Assembly.FullName, null, activityType.Name));
						}
					}
				}
			}
		}

		private void AddCategoryToToolbox(string categoryName, List<Type> activities)
		{
			foreach (Type activityType in activities)
			{
				if (this.IsValidToolboxActivity(activityType))
				{
					ToolboxCategory category = this.GetToolboxCategory(categoryName);

					if (!this.loadedToolboxActivities[category].Contains(activityType.FullName))
					{
						string displayName;
						string[] splitName = activityType.Name.Split('`');
						if (splitName.Length == 1)
						{
							displayName = activityType.Name;
						}
						else
						{
							displayName = string.Format("{0}<>", splitName[0]);
						}

						this.loadedToolboxActivities[category].Add(activityType.FullName);
						category.Add(new ToolboxItemWrapper(activityType.FullName, activityType.Assembly.FullName, null, displayName));
					}
				}
			}
		}

		private void AddActivitiesToToolbox(List<Type> activities)
		{
			foreach (Type activityType in activities)
			{
				if (this.IsValidToolboxActivity(activityType))
				{
					ToolboxCategory category = this.GetToolboxCategory(activityType.Namespace);

					if (!this.loadedToolboxActivities[category].Contains(activityType.FullName))
					{
						this.loadedToolboxActivities[category].Add(activityType.FullName);
						category.Add(new ToolboxItemWrapper(activityType.FullName, activityType.Assembly.FullName, null, activityType.Name));
					}
				}
			}
		}

		private void RemoveCategoryFromToolbox(List<Assembly> assemblies)
		{
			foreach (Assembly assembly in assemblies)
			{
				foreach (Type activityType in assembly.GetTypes())
				{
					if (this.IsValidToolboxActivity(activityType))
					{
						ToolboxCategory category = this.GetToolboxCategory(activityType.Namespace);

						if (!this.loadedToolboxActivities[category].Contains(activityType.FullName))
						{
							this.loadedToolboxActivities[category].Remove(activityType.FullName);
							category.Remove(new ToolboxItemWrapper(activityType.FullName, activityType.Assembly.FullName, null, activityType.Name));
						}
					}
				}
			}
		}

		private List<Type> GetReferencedActivities(ModelService modelService)
		{
			IEnumerable<ModelItem> items = modelService.Find(modelService.Root, typeof(Activity));
			List<Type> activities = new List<Type>();
			foreach (ModelItem item in items)
			{
				if (!namespacesToIgnore.Contains(item.ItemType.Namespace))
				{
					activities.Add(item.ItemType);
				}
			}

			return activities;
		}

		private ToolboxCategory GetToolboxCategory(string name)
		{
			if (this.toolboxCategoryMap.ContainsKey(name))
			{
				return this.toolboxCategoryMap[name];
			}
			else
			{
				ToolboxCategory category = new ToolboxCategory(name);
				this.toolboxCategoryMap[name] = category;
				this.loadedToolboxActivities.Add(category, new List<string>());
				//this.toolboxControl.Categories.Add(category);
				return category;
			}
		}

	}


}
