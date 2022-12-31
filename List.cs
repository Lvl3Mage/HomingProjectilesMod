using CustomList;
using System;
namespace CustomList
{
	public class List<T> // Custom linked list implementation
	{
		ListElement<T> firstElement = null;
		public int Count
		{
			get
			{
				return count;
			}
		}
		int count = 0;

		public T this[int i]
		{
			get => GetValue(i);
			set => SetValue(i,value);
		}
		T GetValue(int id){
			CheckOutOfBounds(id);
			return GetElementById(id).value;
		}
		void SetValue(int id, T value){
			CheckOutOfBounds(id);
			GetElementById(id).value = value;
		}
		public void Add(T value){
			if(count == 0){
				firstElement = new ListElement<T>(value);
			}
			else{
				ListElement<T> lastElement = GetElementById(count - 1);
				lastElement.ConnectTo(new ListElement<T>(value));	
			}
			
			count++;
		}
		public void RemoveAt(int id){
			CheckOutOfBounds(id);
			if(id == 0){
				firstElement = firstElement.Next; // first element falls out of scope
			}
			else{
				ListElement<T> prevElement = GetElementById(id - 1);
				prevElement.ConnectTo(prevElement.Next.Next);
				
			}
			count--;
		}
		public void Insert(T value, int id){
			CheckOutOfBounds(id, 0, count);
			if(id == 0){
				ListElement<T> newFirst = new ListElement<T>(value);
				newFirst.ConnectTo(firstElement);
				firstElement = newFirst;
			}
			else{
				ListElement<T> newElement = new ListElement<T>(value);
				ListElement<T> prevElement = GetElementById(id - 1);

				newElement.ConnectTo(prevElement.Next);
				prevElement.ConnectTo(newElement);
			}
			count++;
		}
		void CheckOutOfBounds(int id){
			if(id < 0 || id > (count-1)){
				throw(new IndexOutOfRangeException("Index was outside the bounds of the list"));
			}
		}
		void CheckOutOfBounds(int id, int min, int max){// With custom bounds
			if(id < min || id > max){
				throw(new IndexOutOfRangeException("Index was outside the bounds of the list"));
			}
		}
		ListElement<T> GetElementById(int id){
			ListElement<T> cursor = firstElement;

			//moves cursor to id
			for(int i = 0; i < id; i++){
				cursor = cursor.Next;
			}

			return cursor;
		}
		public List()
		{

		}
		public List(List<T> inputList)
		{
			for(int i = 0; i < inputList.Count; i++){
				Add(inputList[i]);
			}
		}
		public List(T[] inputArray)
		{
			for(int i = 0; i < inputArray.Length; i++){
				Add(inputArray[i]);
			}
		}
		public T[] ToArray(){
			T[] array = new T[count];
			ListElement<T> cursor = firstElement;
			for(int i = 0; i < count; i++){
				array[i] = firstElement.value;
				cursor = cursor.Next;
			}
			return array;
		}
		
	}
	public class ListElement<ElemT>
	{
		public ElemT value;
		public ListElement<ElemT> Next
		{
			get
			{
				return next;
			}
		}
		ListElement<ElemT> next = null;
		public ListElement(ElemT newValue){
			value = newValue;
		}
		public void ConnectTo(ListElement<ElemT> newNext){
			next = newNext;
		}
	}
}