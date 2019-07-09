using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;

namespace Editor {
    public abstract class Figure : Observable, Observer {
        public abstract void Draw(Pen pen, Graphics graph);
        public abstract bool IsSelected();
        public abstract bool IsGrouped();
        public abstract bool IsSticky();
        public abstract bool IsSticked();
        public abstract void InvertSelected();
        public abstract void ChangeSelected(bool selected);
        public abstract void ChangeGrouped(bool grouped);
        public abstract void ChangeSticky(bool sticky);
        public abstract void ChangeSticked(bool sticked);
        public abstract void Scale(int delta);
        public abstract bool IsObstacle(int width, int height);
        public abstract bool IsTooSmall();
        public abstract void Move(int delta_x, int delta_y);
        public abstract void FillArrayForMove(ref bool[] arr, int width, int height);
        public abstract void UpdatePath();
        public abstract void ChangeColor(Color color);
        public abstract Color GetColor();
        public abstract bool IsClickCatchFigure(Point point);
        public abstract bool IsAreaCatchFigure(Point point1, Point point2);
        public abstract string Classname();
        public abstract void Save(StreamWriter sw);
        public abstract Figure Load(StreamReader sr, FigureFactory factory);
        public abstract void AddObserver(Observer observer);
        public abstract void RemoveObserver(Observer observer);
        public abstract void RemoveObservers();
        public abstract void NotifyAboutMove();
        public abstract void NotifyAboutObstacle();
        public abstract void NotifyAboutChangeInStorage();
        public abstract void NotifyAboutSelectInTree();
        public abstract void MoveUpdate(Observable observable);
        public abstract void ObstacleUpdate(Observable observable);
        public abstract void ChangeInStorageUpdate(Observable observable);
        public abstract void SelectInTreeUpdate(Observable observable);
        public abstract bool IsToStick(Figure sticky);
        public abstract bool IsFigureStickedToGroup(Figure figure);
        public abstract bool IsGroupStickedToGroup(Figure figure);
        public abstract Figure Search(int id);
        public abstract string GetID();
        protected static int ID = 0;
        protected List<Observer> observers = new List<Observer>();
        public List<Observer> GetObservers() {
            return observers;
        }
        protected bool[] obstacles = new bool[5];
        public ref bool[] GetObstacles() {
            return ref obstacles;
        }
        public void ResetObstacles() {
            for (int i = 0; i < 5; i++) {
                obstacles[i] = false;
            }
        }
    }

    public class Group : Figure {
        protected Figure[] ptr;
        protected int size;
        protected int capacity;
        private int id;
        private Color color;
        private bool selected;
        private bool grouped;
        private bool sticky;
        private bool sticked;
        private int lastDeltaX;
        private int lastDeltaY;

        public int GetlastDeltaX() {
            return lastDeltaX;
        }

        public int GetlastDeltaY() {
            return lastDeltaY;
        }

        public Group() {
            color = Color.Green;
            ptr = null;
            size = 0;
            capacity = 0;
            id = ID++;
        }

        public int Count() {
            return size;
        }

        public Figure this[int index] {
            get {
                return ptr[index];
            }
            set {
                ptr[index] = value;
            }
        }

        public virtual void Add(Figure obj) {
            if (size == capacity) {
                Figure[] copy = ptr;
                capacity = (int)((capacity + 1) * 1.25);
                ptr = new Figure[capacity];
                for (int i = 0; i < size; i++) {
                    ptr[i] = copy[i];
                }
                ptr[size] = obj;
                size++;
                return;
            }
            ptr[size] = obj;
            size++;
        }

        public override void Draw(Pen pen, Graphics graph) {
            for (int i = 0; i < size; i++) {
                this[i].Draw(pen, graph);
            }
        }

        public override bool IsSelected() {
            return selected;
        }

        public override bool IsGrouped() {
            return grouped;
        }

        public override bool IsSticky() {
            return sticky;
        }

        public override bool IsSticked() {
            return sticked;
        }

        public override void ChangeSelected(bool selected) {
            this.selected = selected;
            for (int i = 0; i < size; i++) {
                this[i].ChangeSelected(selected);
            }
            NotifyAboutChangeInStorage();
        }

        public override void InvertSelected() {
            if (selected) {
                selected = false;
            }
            else {
                selected = true;
            }
            for (int i = 0; i < size; i++) {
                this[i].InvertSelected();
            }
            NotifyAboutChangeInStorage();
        }

        public override void ChangeGrouped(bool grouped) {
            this.grouped = grouped;
            for (int i = 0; i < size; i++) {
                this[i].ChangeGrouped(grouped);
            }
        }

        public override void ChangeSticky(bool sticky) {
            this.sticky = sticky;
            for (int i = 0; i < size; i++) {
                this[i].ChangeSticky(sticky);
            }
        }

        public override void ChangeSticked(bool sticked) {
            this.sticked = sticked;
            for (int i = 0; i < size; i++) {
                this[i].ChangeSticked(sticked);
            }
        }

        public override void Scale(int delta) {
            for (int i = 0; i < size; i++) {
                this[i].Scale(delta);
            }
        }

        public override bool IsObstacle(int width, int height) {
            for (int i = 0; i < size; i++) {
                if (this[i].IsObstacle(width, height)) {
                    return true;
                }
            }
            return false;
        }

        public override bool IsTooSmall() {
            for (int i = 0; i < size; i++) {
                if (this[i].IsTooSmall()) {
                    return true;
                }
            }
            return false;
        }

        public override void Move(int delta_x, int delta_y) {
            lastDeltaX = delta_x;
            lastDeltaY = delta_y;
            for (int i = 0; i < size; i++) {
                this[i].Move(delta_x, delta_y);
            }
            if (sticky) {
                NotifyAboutMove();
            }
        }

        public override void FillArrayForMove(ref bool[] arr, int width, int height) {
            for (int i = 0; i < size; i++) {
                this[i].FillArrayForMove(ref arr, width, height);
            }
        }

        public override void UpdatePath() {
            for (int i = 0; i < size; i++) {
                this[i].UpdatePath();
            }
        }

        public override void ChangeColor(Color color) {
            this.color = color;
            for (int i = 0; i < size; i++) {
                this[i].ChangeColor(color);
            }
        }

        public override Color GetColor() {
            return color;
        }

        public override bool IsClickCatchFigure(Point point) {
            for (int i = 0; i < size; i++) {
                if (this[i].IsClickCatchFigure(point)) {
                    return true;
                }
            }
            return false;
        }

        public override bool IsAreaCatchFigure(Point point1, Point point2) {
            for (int i = 0; i < size; i++) {
                if (this[i].IsAreaCatchFigure(point1, point2)) {
                    return true;
                }
            }
            return false;
        }

        public override string Classname() {
            return "Group";
        }

        public override void Save(StreamWriter sw) {
            sw.Write("Group, size = ");
            sw.WriteLine(size);
            sw.WriteLine();
            for (int i = 0; i < size; i++) {
                this[i].Save(sw);
            }
            sw.WriteLine("Group is over");
            sw.WriteLine();
        }

        public override Figure Load(StreamReader sr, FigureFactory factory) {
            string str = sr.ReadLine();
            sr.ReadLine();
            int count = Convert.ToInt32(str.Substring(13, str.Length - 13));
            for (int i = 0; i < count; i++) {
                char symbol = (char)sr.Read();
                Figure figure = factory.CreateFigure(symbol);
                Add(figure.Load(sr, factory));
            }
            sr.ReadLine();
            sr.ReadLine();
            return this;
        }

        public override void AddObserver(Observer observer) {
            observers.Add(observer);
        }

        public override void RemoveObserver(Observer observer) {
            for (int i = 0; i < observers.Count; i++) {
                if (observer == observers[i]) {
                    observers.RemoveAt(i);
                    return;
                }
            }
        }

        public override void RemoveObservers() {
            for (int i = observers.Count - 1; i > 0; i--) {
                ((Figure)observers[i]).ChangeSticked(false);
                observers.RemoveAt(i);
            }
        }

        public override bool IsToStick(Figure sticky) {
            if (sticky is MyPoint) {
                if (IsFigureStickedToGroup(sticky)) {
                    return true;
                }
                return false;
            }
            else {
                if (IsGroupStickedToGroup(sticky)) {
                    return true;
                }
                return false;
            }
        }

        public override bool IsFigureStickedToGroup(Figure figure) {
            for (int i = 0; i < size; i++) {
                if (this[i].IsFigureStickedToGroup(figure)) {
                    return true;
                }
            }
            return false;
        }

        public override bool IsGroupStickedToGroup(Figure figure) {
            for (int i = 0; i < size; i++) {
                if (this[i].IsGroupStickedToGroup(figure)) {
                    return true;
                }
            }
            return false;
        }

        public override void NotifyAboutMove() {
            for (int i = 0; i < observers.Count; i++) {
                observers[i].MoveUpdate(this);
            }
        }

        public override void NotifyAboutObstacle() {
            for (int i = 0; i < observers.Count; i++) {
                observers[i].ObstacleUpdate(this);
            }
        }

        public override void NotifyAboutChangeInStorage() {
            for (int i = 0; i < observers.Count; i++) {
                observers[i].ChangeInStorageUpdate(this);
            }
        }

        public override void NotifyAboutSelectInTree() {
            //////////
        }

        public override void MoveUpdate(Observable observable) {
            if (observable is MyPoint) {
                Move(((MyPoint)observable).GetlastDeltaX(), ((MyPoint)observable).GetlastDeltaY());
            }
            else {
                Move(((Group)observable).GetlastDeltaX(), ((Group)observable).GetlastDeltaY());
            }
        }

        public override void ObstacleUpdate(Observable observable) {
            for (int i = 0; i < 5; i++) {
                if (((Figure)observable).GetObstacles()[i]) {
                    obstacles[i] = true;
                }
            }
        }

        public override void ChangeInStorageUpdate(Observable observable) {
            //////////
        }

        public override void SelectInTreeUpdate(Observable observable) {
            //////////
        }

        public override Figure Search(int id) {
            if (this.id == id) {
                return this;
            }
            for (int i = 0; i < size; i++) {
                if (Convert.ToInt32(this[i].Search(id).GetID()) != -1) {
                    return this[i];
                }
            }
            Group group = new Group();
            group.id = -1;
            return group;
        }

        public override string GetID() {
            return Convert.ToString(id);
        }
    }

    public class MyPoint : Figure {
        protected int x;
        protected int y;
        protected int size;
        protected bool selected;
        protected bool grouped;
        protected bool sticky;
        protected bool sticked;
        protected GraphicsPath path;
        protected Color color;
        private int id;
        private int lastDeltaX;
        private int lastDeltaY;

        public int GetlastDeltaX() {
            return lastDeltaX;
        }

        public int GetlastDeltaY() {
            return lastDeltaY;
        }

        public GraphicsPath GetPath() {
            return path;
        }

        public MyPoint() {
            id = ID++;
        }

        public override void Draw(Pen pen, Graphics graph) {
            pen.Color = color;
            if (sticky) {
                pen.Color = Color.Purple;
            }
            if (selected) {
                pen.Color = Color.Red;
            }
            graph.DrawPath(pen, path);
        }

        public override bool IsSelected() {
            return selected;
        }

        public override bool IsGrouped() {
            return grouped;
        }

        public override bool IsSticky() {
            return sticky;
        }

        public override bool IsSticked() {
            return sticked;
        }

        public override void ChangeSelected(bool selected) {
            this.selected = selected;
            NotifyAboutChangeInStorage();
        }

        public override void InvertSelected() {
            if (selected) {
                selected = false;
            }
            else {
                selected = true;
            }
            NotifyAboutChangeInStorage();
        }

        public override void ChangeGrouped(bool grouped) {
            this.grouped = grouped;
        }

        public override void ChangeSticky(bool sticky) {
            this.sticky = sticky;
        }

        public override void ChangeSticked(bool sticked) {
            this.sticked = sticked;
        }

        public override void Scale(int delta) {
                size += delta;
        }

        public override bool IsObstacle(int width, int height) {
            RectangleF rect = path.GetBounds();
            float w = rect.Width;
            float h = rect.Height;
            return (rect.X < 10) || (rect.Y < 10) || (rect.X + w > (width - 10)) || (rect.Y + h > (height - 10));
        }

        public override bool IsTooSmall() {
            return (size == 20);
        }

        public override void Move(int delta_x, int delta_y) {
            lastDeltaX = delta_x;
            lastDeltaY = delta_y;
            x += delta_x;
            y += delta_y;
            if (sticky) {
                NotifyAboutMove();
            }
        }

        public override void FillArrayForMove(ref bool[] arr, int width, int height) {
            RectangleF rect = path.GetBounds();
            float w = rect.Width;
            float h = rect.Height;
            if (rect.X < 10) {
                arr[1] = true;
            }
            if (rect.Y < 10) {
                arr[2] = true;
            }
            if (rect.X + w > (width - 10)) {
                arr[3] = true;
            }
            if (rect.Y + h > (height - 10)) {
                arr[4] = true;
            }
        }

        public override void UpdatePath() {

        }

        public override void ChangeColor(Color color) {
            this.color = color;
        }

        public override Color GetColor() {
            return color;
        }

        public override bool IsClickCatchFigure(Point point) {
            Pen pen = new Pen(Color.Green, 4);
            return path.IsOutlineVisible(point, pen);
        }

        public override bool IsAreaCatchFigure(Point point1, Point point2) {
            PointF[] points = path.PathPoints;
            for (int i = 0; i < points.Length; i++) {
                if ((points[i].X > point1.X) && (points[i].Y > point1.Y) && (points[i].X < point2.X) && (points[i].Y < point2.Y)) {
                    return true;
                }
            }
            return false;
        }

        public override string Classname() {
            return "MyPoint";
        }

        public override void Save(StreamWriter sw) {

        }

        public override Figure Load(StreamReader sr, FigureFactory factory) {
            return new MyPoint();
        }

        public override void AddObserver(Observer observer) {
            observers.Add(observer);
        }

        public override void RemoveObserver(Observer observer) {
            for (int i = observers.Count - 1; i >= 0; i--) {
                if (observer == observers[i]) {
                    observers.RemoveAt(i);
                    return;
                }
            }
        }

        public override void RemoveObservers() {
            for (int i = observers.Count - 1; i > 0; i--) {
                ((Figure)observers[i]).ChangeSticked(false);
                observers.RemoveAt(i);
            }
        }

        public override void NotifyAboutMove() {
            for (int i = 0; i < observers.Count; i++) {
                observers[i].MoveUpdate(this);
            }
        }

        public override void NotifyAboutObstacle() {
            for (int i = 0; i < observers.Count; i++) {
                observers[i].ObstacleUpdate(this);
            }
        }

        public override void NotifyAboutChangeInStorage() {
            for (int i = 0; i < observers.Count; i++) {
                observers[i].ChangeInStorageUpdate(this);
            }
        }

        public override void NotifyAboutSelectInTree() {
            //////////
        }

        public override void MoveUpdate(Observable observable) {
            if (observable is MyPoint) {
                Move(((MyPoint)observable).GetlastDeltaX(), ((MyPoint)observable).GetlastDeltaY());
            }
            else {
                Move(((Group)observable).GetlastDeltaX(), ((Group)observable).GetlastDeltaY());
            }
        }

        public override void ObstacleUpdate(Observable observable) {
            for (int i = 0; i < 5; i++) {
                if (((Figure)observable).GetObstacles()[i]) {
                    obstacles[i] = true;
                }
            }
        }

        public override void ChangeInStorageUpdate(Observable observable) {
            //////////
        }

        public override void SelectInTreeUpdate(Observable observable) {
            //////////
        }

        public override bool IsToStick(Figure sticky) {
            if (sticky is MyPoint) {
                RectangleF rect = ((MyPoint)sticky).GetPath().GetBounds();
                PointF[] points = path.PathPoints;
                for (int i = 0; i < points.Length; i++) {
                    if ((points[i].X > rect.X) && (points[i].X < (rect.X + rect.Width)) && (points[i].Y > rect.Y) && (points[i].Y < (rect.Y + rect.Height))) {
                        return true;
                    }
                }
                return false;
            }
            if (sticky.IsFigureStickedToGroup(this)) {
                return true;
            }
            return false;
        }

        public override bool IsFigureStickedToGroup(Figure figure) {
            RectangleF rect = path.GetBounds();
            PointF[] points = ((MyPoint)figure).GetPath().PathPoints;
            for (int i = 0; i < points.Length; i++) {
                if ((points[i].X > rect.X) && (points[i].X < (rect.X + rect.Width)) && (points[i].Y > rect.Y) && (points[i].Y < (rect.Y + rect.Height))) {
                    return true;
                }
            }
            return false;
        }

        public override bool IsGroupStickedToGroup(Figure figure) {
            if (figure.IsFigureStickedToGroup(this)) {
                return true;
            }
            return false;
        }

        public override Figure Search(int id) {
            if (this.id == id) {
                return this;
            }
            MyPoint mypoint = new MyPoint();
            mypoint.id = -1;
            return mypoint;
        }

        public override string GetID() {
            return Convert.ToString(id);
        }
    }

    public class Circle : MyPoint { 
        public Circle(int x, int y) { 
            this.x = x;
            this.y = y;
            size = 20;
            selected = false;
            color = Color.Green;
            path = new GraphicsPath();
            path.AddEllipse(x - size, y - size, size * 2, size * 2);
        }

        public Circle(int x, int y, int r, Color color) {
            this.x = x;
            this.y = y;
            size = r;
            selected = false;
            this.color = color;
            path = new GraphicsPath();
            path.AddEllipse(x - r, y - r, r * 2, r * 2);
        }

        public override void UpdatePath() {
            path.Reset();
            path.AddEllipse(x - size, y - size, size * 2, size * 2);
        }

        public override string Classname() {
            return "Circle";
        }

        public override void Save(StreamWriter sw) {
            sw.WriteLine("Circle");
            sw.WriteLine();
            sw.WriteLine(x);
            sw.WriteLine(y);
            sw.WriteLine(size);
            sw.WriteLine(color.A);
            sw.WriteLine(color.R);
            sw.WriteLine(color.G);
            sw.WriteLine(color.B);
            sw.WriteLine();
        }

        public override Figure Load(StreamReader sr, FigureFactory factory) {
            sr.ReadLine();
            sr.ReadLine();
            int x = Convert.ToInt32(sr.ReadLine());
            int y = Convert.ToInt32(sr.ReadLine());
            int size = Convert.ToInt32(sr.ReadLine());
            byte A = Convert.ToByte(sr.ReadLine());
            byte R = Convert.ToByte(sr.ReadLine());
            byte G = Convert.ToByte(sr.ReadLine());
            byte B = Convert.ToByte(sr.ReadLine());
            sr.ReadLine();
            Color color = Color.FromArgb(A, R, G, B);
            return new Circle(x, y, size, color);
        }
    }

    public class Rectangle : MyPoint { 
        public Rectangle(int x, int y) {
            this.x = x;
            this.y = y;
            size = 20;
            selected = false;
            color = Color.Green;
            path = new GraphicsPath();
            System.Drawing.Rectangle rect = new System.Drawing.Rectangle(x - size * 2 / 2, y - size / 2, size * 2, size);
            path.AddRectangle(rect);
        }

        public Rectangle(int x, int y, int height, Color color) {
            this.x = x;
            this.y = y;
            size = height;
            selected = false;
            this.color = color;
            path = new GraphicsPath();
            System.Drawing.Rectangle rect = new System.Drawing.Rectangle(x - size * 2 / 2, y - height / 2, size * 2, height);
            path.AddRectangle(rect);
        }

        public override void UpdatePath() {
            path.Reset();
            System.Drawing.Rectangle rect = new System.Drawing.Rectangle(x - size * 2 / 2, y - size / 2, size * 2, size);
            path.AddRectangle(rect);
        }

        public override string Classname() {
            return "Rectangle";
        }

        public override void Save(StreamWriter sw) {
            sw.WriteLine("Rectangle");
            sw.WriteLine();
            sw.WriteLine(x);
            sw.WriteLine(y);
            sw.WriteLine(size);
            sw.WriteLine(color.A);
            sw.WriteLine(color.R);
            sw.WriteLine(color.G);
            sw.WriteLine(color.B);
            sw.WriteLine();
        }

        public override Figure Load(StreamReader sr, FigureFactory factory) {
            sr.ReadLine();
            sr.ReadLine();
            int x = Convert.ToInt32(sr.ReadLine());
            int y = Convert.ToInt32(sr.ReadLine());
            int size = Convert.ToInt32(sr.ReadLine());
            byte A = Convert.ToByte(sr.ReadLine());
            byte R = Convert.ToByte(sr.ReadLine());
            byte G = Convert.ToByte(sr.ReadLine());
            byte B = Convert.ToByte(sr.ReadLine());
            sr.ReadLine();
            Color color = Color.FromArgb(A, R, G, B);
            return new Rectangle(x, y, size, color);
        }
    }

    public class Section : MyPoint {
        public Section(int x, int y) {
            this.x = x;
            this.y = y;
            size = 40;
            selected = false;
            color = Color.Green;
            path = new GraphicsPath();
            path.AddLine(x - size / 2, y, x + size / 2, y);
        }

        public Section(int x, int y, int length, Color color) {
            this.x = x;
            this.y = y;
            size = length;
            selected = false;
            this.color = color;
            path = new GraphicsPath();
            path.AddLine(x - length / 2, y, x + length / 2, y);
        }

        public override void UpdatePath() {
            path.Reset();
            path.AddLine(x - size / 2, y, x + size / 2, y);
        }

        public override string Classname() {
            return "Section";
        }

        public override void Save(StreamWriter sw) {
            sw.WriteLine("Section");
            sw.WriteLine();
            sw.WriteLine(x);
            sw.WriteLine(y);
            sw.WriteLine(size);
            sw.WriteLine(color.A);
            sw.WriteLine(color.R);
            sw.WriteLine(color.G);
            sw.WriteLine(color.B);
            sw.WriteLine();
        }

        public override Figure Load(StreamReader sr, FigureFactory factory) {
            sr.ReadLine();
            sr.ReadLine();
            int x = Convert.ToInt32(sr.ReadLine());
            int y = Convert.ToInt32(sr.ReadLine());
            int size = Convert.ToInt32(sr.ReadLine());
            byte A = Convert.ToByte(sr.ReadLine());
            byte R = Convert.ToByte(sr.ReadLine());
            byte G = Convert.ToByte(sr.ReadLine());
            byte B = Convert.ToByte(sr.ReadLine());
            sr.ReadLine();
            Color color = Color.FromArgb(A, R, G, B);
            return new Section(x, y, size, color);
        }
    }

    public class Triangle : MyPoint {

        public Triangle(int x, int y) {
            this.x = x;
            this.y = y;
            size = 100;
            selected = false;
            color = Color.Green;
            path = new GraphicsPath();
            int radius = (int)(size / Math.Sqrt(3));
            Point[] points = new Point[4];
            points[0] = new Point(x, y - radius);
            points[1] = new Point(x + radius / 2, y + radius / 2);
            points[2] = new Point(x - radius / 2, y + radius / 2);
            points[3] = points[0];
            path.AddLines(points);
        }

        public Triangle(int x, int y, int side, Color color) {
            this.x = x;
            this.y = y;
            size = side;
            selected = false;
            this.color = color;
            path = new GraphicsPath();
            int radius = (int)(side / Math.Sqrt(3));
            Point[] points = new Point[4];
            points[0] = new Point(x, y - radius);
            points[1] = new Point(x + radius / 2, y + radius / 2);
            points[2] = new Point(x - radius / 2, y + radius / 2);
            points[3] = points[0];
            path.AddLines(points);
        }

        public override void UpdatePath() {
            path.Reset();
            int radius = (int)(size / Math.Sqrt(3));
            Point[] points = new Point[4];
            points[0] = new Point(x, y - radius);
            points[1] = new Point(x + radius / 2, y + radius / 2);
            points[2] = new Point(x - radius / 2, y + radius / 2);
            points[3] = points[0];
            path.AddLines(points);
        }

        public override string Classname() {
            return "Triangle";
        }

        public override void Save(StreamWriter sw) {
            sw.WriteLine("Triangle");
            sw.WriteLine();
            sw.WriteLine(x);
            sw.WriteLine(y);
            sw.WriteLine(size);
            sw.WriteLine(color.A);
            sw.WriteLine(color.R);
            sw.WriteLine(color.G);
            sw.WriteLine(color.B);
            sw.WriteLine();
        }

        public override Figure Load(StreamReader sr, FigureFactory factory) {
            sr.ReadLine();
            sr.ReadLine();
            int x = Convert.ToInt32(sr.ReadLine());
            int y = Convert.ToInt32(sr.ReadLine());
            int size = Convert.ToInt32(sr.ReadLine());
            byte A = Convert.ToByte(sr.ReadLine());
            byte R = Convert.ToByte(sr.ReadLine());
            byte G = Convert.ToByte(sr.ReadLine());
            byte B = Convert.ToByte(sr.ReadLine());
            sr.ReadLine();
            Color color = Color.FromArgb(A, R, G, B);
            return new Triangle(x, y, size, color);
        }
    }
}
