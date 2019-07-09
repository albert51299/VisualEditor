using System;
using System.Collections.Generic;
using System.IO;

namespace Editor {
    public class MyVector : Group {
        public override void Add(Figure obj) {
            if (!obj.GetObservers().Contains(this)) {
                obj.AddObserver(this);
            }
            if (size == capacity) {
                Figure[] copy = ptr;
                capacity = (int)((capacity + 1) * 1.25);
                ptr = new Figure[capacity];
                for (int i = 0; i < size; i++) {
                    ptr[i] = copy[i];
                }
                ptr[size] = obj;
                size++;
                NotifyAboutChangeInStorage();
                return;
            }
            ptr[size] = obj;
            size++;
            NotifyAboutChangeInStorage();
        }

        public void RemoveAt(int index) {
            this[index].RemoveObservers();
            size--;
            for (int i = index; i < size; i++) {
                ptr[i] = ptr[i + 1];
            }
            ptr[size] = null;
            NotifyAboutChangeInStorage();
        }

        public void Remove(Figure figure) {
            for (int i = 0; i < size; i++) {
                if (this[i] == figure) {
                    RemoveAt(i);
                }
            }
        }

        public override string Classname() {
            return "MyVector";
        }

        public override void Save(StreamWriter sw) {
            sw.WriteLine(size);
            sw.WriteLine();
            for (int i = 0; i < size; i++) {
                this[i].Save(sw);
            }
        }

        public override Figure Load(StreamReader sr, FigureFactory factory) {
            int count = Convert.ToInt32(sr.ReadLine());
            sr.ReadLine();
            for (int i = 0; i < count; i++) {
                char symbol = (char)sr.Read();
                Figure figure = factory.CreateFigure(symbol);
                Add(figure.Load(sr, factory));
            }
            return this;
        }

        public override void NotifyAboutMove() {
            //////////
        }

        public override void NotifyAboutChangeInStorage() {
            for (int i = 0; i < observers.Count; i++) {
                observers[i].ChangeInStorageUpdate(this);
            }
        }

        public override void MoveUpdate(Observable observable) {
            //////////
        }

        public override void ChangeInStorageUpdate(Observable observable) {
            NotifyAboutChangeInStorage();
        }

        public override void SelectInTreeUpdate(Observable observable) {
            int id = ((EditorForm)observable).GetID();
            if (id == 0) {
                for (int i = 0; i < size; i++) {
                    this[i].ChangeSelected(true);
                }
            }
            else {
                for (int i = 0; i < size; i++) {
                    this[i].ChangeSelected(false);
                }
                Figure figure = Search(id);
                figure.ChangeSelected(true);
            }
        }
    }
}
