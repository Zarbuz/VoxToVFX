using UnityEngine;
using UnityEngine.UI;
using System;
using UnityEngine.EventSystems;

namespace Com.TheFallenGames.OSA.AdditionalComponents
{
	/// <summary>
	/// Utility that allows dragging a ScrollRect even if the PointerDown event has started inside a child InputField (which cancels the dragging by default)
	/// </summary>
	public abstract class InputFieldInScrollRectFixerBase : MonoBehaviour, IInitializePotentialDragHandler, IBeginDragHandler, IDragHandler, IEndDragHandler, IPointerClickHandler, IPointerDownHandler, IPointerUpHandler
	{
		protected Selectable _InputField;
		Image _ImageOnMeIfChild;
		const string CHILD_NAME = "InputFieldFixer-Child";

		bool _IAmChild;
		bool _DragInProgress;


		protected virtual void Awake()
		{
			_InputField = GetComponent<Selectable>();
			_IAmChild = _InputField == null;
			if (_IAmChild)
			{
				InitAsChild();
			}
			else
			{
				CacheMethods();
				InitAsParent();
			}
		}

		protected abstract void CacheMethods();

		void OnDisable()
		{
			_DragInProgress = false;
		}

		void InitAsParent()
		{
			var inputFieldImg = _InputField.image;
			if (inputFieldImg)
				inputFieldImg.raycastTarget = false;

			var tr = transform.Find(CHILD_NAME);
			GameObject go;
			RectTransform goRT;

			// The child may already exist if you'll instantiate an existing InputField with InputFieldInScrollRectFixer attached
			if (tr == null)
			{
				go = new GameObject(CHILD_NAME, typeof(RectTransform));
				goRT = go.transform as RectTransform;
				goRT.SetParent(_InputField.transform, false);
				go.AddComponent(GetType() /*add the same component as <this>'s type, i.e. the one for InputField or TMPro.TMP_InputField*/);
			}

			// Parent not needed anymore
			Destroy(this);
		}

		void InitAsChild()
		{
			name = CHILD_NAME;
			_InputField = transform.parent.GetComponent<Selectable>();
			if (!_InputField)
				throw new InvalidOperationException("Child InputFieldInScrollRectFixer: InputField not found in parent");

			CacheMethods();

			var inputFieldImg = _InputField.image;
			if (!inputFieldImg)
				throw new InvalidOperationException("Child InputFieldInScrollRectFixer: InputField must have an image attached (can be invisible)");

			// May have already been created if this is an instance of a another runtime instance
			_ImageOnMeIfChild = GetComponent<Image>();
			if (!_ImageOnMeIfChild)
			{
				_ImageOnMeIfChild = gameObject.AddComponent<Image>();
				_ImageOnMeIfChild.sprite = inputFieldImg.sprite;
			}

			var goRT = transform as RectTransform;

			goRT.SetAsLastSibling();
			goRT.anchorMin = Vector2.zero;
			goRT.anchorMax = Vector2.one;
			goRT.sizeDelta = Vector2.zero;

			_ImageOnMeIfChild.color = Color.clear;
		}

		protected abstract void ActivateInputField();
		protected abstract bool IsInputFieldFocused();

		void IPointerDownHandler.OnPointerDown(PointerEventData eventData)
		{
			var par = GetInputFieldIfActiveOrNextComponentInItsParents<IPointerDownHandler>();
			if (par != null)
				par.OnPointerDown(eventData);
		}

		void IPointerUpHandler.OnPointerUp(PointerEventData eventData)
		{
			var par = GetInputFieldIfActiveOrNextComponentInItsParents<IPointerUpHandler>();
			if (par != null)
				par.OnPointerUp(eventData);
		}

		void IPointerClickHandler.OnPointerClick(PointerEventData eventData)
		{
			if (InputFieldActiveAndFocused())
			{
				(_InputField as IPointerClickHandler).OnPointerClick(eventData);
				return;
			}
			
			if (_DragInProgress)
				return;

			if (eventData.useDragThreshold)
			{
				var dragDist = Vector2.Distance(eventData.pressPosition, eventData.position);
				if (dragDist > EventSystem.current.pixelDragThreshold)
					return;
			}

			if (CanInputFieldBeFocused())
			{
				ActivateInputField();
				return;
			}

			var par = GetComponentInInputFieldParents<IPointerClickHandler>();
			if (par != null)
				par.OnPointerClick(eventData);
		}

		void IInitializePotentialDragHandler.OnInitializePotentialDrag(PointerEventData eventData)
		{
			if (!InputFieldActiveAndFocused())
			{
				var par = GetComponentInInputFieldParents<IInitializePotentialDragHandler>();
				if (par != null)
					par.OnInitializePotentialDrag(eventData);
			}
		}

		void IBeginDragHandler.OnBeginDrag(PointerEventData eventData)
		{
			_DragInProgress = true;
			var par = GetInputFieldIfActiveOrNextComponentInItsParents<IBeginDragHandler>();
			if (par != null)
				par.OnBeginDrag(eventData);
		}

		void IDragHandler.OnDrag(PointerEventData eventData)
		{
			var par = GetInputFieldIfActiveOrNextComponentInItsParents<IDragHandler>();
			if (par != null)
				par.OnDrag(eventData);
		}

		void IEndDragHandler.OnEndDrag(PointerEventData eventData)
		{
			_DragInProgress = false;
			var par = GetInputFieldIfActiveOrNextComponentInItsParents<IEndDragHandler>();
			if (par != null)
				par.OnEndDrag(eventData);
		}

		bool InputFieldActiveAndFocused() { return CanInputFieldBeFocused() && IsInputFieldFocused(); }
		bool CanInputFieldBeFocused() { return _InputField.isActiveAndEnabled && _InputField.interactable; }

		T GetInputFieldIfActiveOrNextComponentInItsParents<T>()
		{
			if (InputFieldActiveAndFocused())
				return (T)(object)_InputField;

			return GetComponentInInputFieldParents<T>();
		}

		T GetComponentInInputFieldParents<T>() { return (T)(object)_InputField.transform.parent.GetComponentInParent(typeof(T)); }
	}
}
