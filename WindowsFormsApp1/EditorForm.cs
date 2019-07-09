using System;
using System.Drawing;
using System.Windows.Forms;
using System.IO;
using System.Collections.Generic;

namespace Editor {
    public partial class EditorForm : Form, Observer, Observable {
        public enum Figures {
            Circle, Rectangle, Section, Triangle, Nothing
        }

        private Figures state = new Figures();
        private MyVector vector = new MyVector();
        private Point point1 = new Point(0, 0);
        private Point point2 = new Point(0, 0);
        private Point point3 = new Point(0, 0);
        private Stack<Command> history = new Stack<Command>();
        private List<Observer> observers = new List<Observer>();
        private string str = "file.txt";
        private int id = -1;
        private bool fromStorage = false;

        Command addCommand = new AddCommand();
        Command removeCommand = new RemoveCommand();
        Command increaseCommand = new ScaleCommand(10);
        Command decreaseCommand = new ScaleCommand(-10);
        Command leftCommand = new MoveCommand(-10, 0);
        Command rightCommand = new MoveCommand(10, 0);
        Command upCommand = new MoveCommand(0, -10);
        Command downCommand = new MoveCommand(0, 10);
        Command invertSelectedCommand = new InvertSelectedCommand();
        Command trueSelectedCommand = new ChangeSelectedCommand(true);
        Command greenCommand = new ChangeColorCommand(Color.Green);
        Command blackCommand = new ChangeColorCommand(Color.Black);
        Command blueCommand = new ChangeColorCommand(Color.Blue);
        Command groupCommand = new GroupCommand();
        Command ungroupCommand = new UngroupCommand();
        Command stickyCommand = new StickyCommand();
        Command notStickyCommand = new NotStickyCommand();
        Command subscribeCommand = new SubscribeCommand();

        public MyVector GetStorage() {
            return vector;
        }

        public Point GetPointForAdd() {
            return point3;
        }

        public Point GetFirstPointForSelected() {
            return point1;
        }

        public Point GetSecondPointForSelected() {
            return point2;
        }

        public Figures GetState() {
            return state;
        }

        public void ChangeState(Figures state) {
            this.state = state;
        }

        public int GetPanelWidth() {
            return panel1.Width;
        }

        public int GetPanelHeight() {
            return panel1.Height;
        }

        public EditorForm() {
            InitializeComponent();
            state = Figures.Nothing;
            vector.AddObserver(this);
            AddObserver(vector);
        }

        private void panel1_Click(object sender, EventArgs e) {
            point3.X = ((MouseEventArgs)e).X;
            point3.Y = ((MouseEventArgs)e).Y;
            Command newCommand = addCommand.Clone();
            if (newCommand.Execute(this)) {
                history.Push(newCommand);
                Subscribe();
                panel1.Refresh();
            }
        }

        private void panel1_Paint(object sender, PaintEventArgs e) {
            Graphics graph = panel1.CreateGraphics();
            Pen pen = new Pen(Color.Green, 4);
            vector.Draw(pen, graph);
            pen.Dispose();
            graph.Dispose();
        }

        private void EditorForm_KeyDown(object sender, KeyEventArgs e) {
            switch (e.KeyData) {
                case Keys.Delete:
                    Command newCommand = removeCommand.Clone();
                    if (newCommand.Execute(this)) {
                        history.Push(newCommand);
                        Subscribe();
                    }
                    break;
                case Keys.Oemplus:
                    Command newIncrCommand = increaseCommand.Clone();
                    if (newIncrCommand.Execute(this)) {
                        history.Push(newIncrCommand);
                        Subscribe();
                    }
                    break;
                case Keys.OemMinus:
                    Command newDecrCommand = decreaseCommand.Clone();
                    if (newDecrCommand.Execute(this)) {
                        history.Push(newDecrCommand);
                        Subscribe();
                    }
                    break;
                case Keys.Left:
                    Command newLeftCommand = leftCommand.Clone();
                    if (newLeftCommand.Execute(this)) {
                        history.Push(newLeftCommand);
                        Subscribe();
                    }
                    break;
                case Keys.Right:
                    Command newRightCommand = rightCommand.Clone();
                    if (newRightCommand.Execute(this)) {
                        history.Push(newRightCommand);
                        Subscribe();
                    }
                    break;
                case Keys.Up:
                    Command newUpCommand = upCommand.Clone();
                    if (newUpCommand.Execute(this)) {
                        history.Push(newUpCommand);
                        Subscribe();
                    }
                    break;
                case Keys.Down:
                    Command newDownCommand = downCommand.Clone();
                    if (newDownCommand.Execute(this)) {
                        history.Push(newDownCommand);
                        Subscribe();
                    }
                    break;
                case Keys.Z:
                    if (history.Count == 0) {
                        break;
                    }
                    if (history.Peek() is SubscribeCommand) {
                        history.Pop().Unexecute();
                    }
                    history.Pop().Unexecute();
                    break;
            }
            vector.UpdatePath();
            panel1.Refresh();
        }

        private void panel1_MouseDown(object sender, MouseEventArgs e) {
            point1.X = e.X;
            point1.Y = e.Y;
        }

        private void panel1_MouseUp(object sender, MouseEventArgs e) {
            point2.X = e.X;
            point2.Y = e.Y;
            if ((point1.X == point2.X) && (point1.Y == point2.Y)) {
                Command newInvertSelectedCommand = invertSelectedCommand.Clone();
                if (newInvertSelectedCommand.Execute(this)) {
                    history.Push(newInvertSelectedCommand);
                    panel1.Refresh();
                }
            }
            else {
                CreateArea(ref point1, ref point2);
                Command newTrueSelectedCommand = trueSelectedCommand.Clone();
                if (newTrueSelectedCommand.Execute(this)) {
                    history.Push(newTrueSelectedCommand);
                    panel1.Refresh();
                }
            }
        }

        private void CreateArea(ref Point point1, ref Point point2) {
            if (point1.X > point2.X) {
                int tmp = point1.X;
                point1.X = point2.X;
                point2.X = tmp;
            }
            if (point1.Y > point2.Y) {
                int tmp = point1.Y;
                point1.Y = point2.Y;
                point2.Y = tmp;
            }
        }

        private void groupSelectedToolStripMenuItem_Click(object sender, EventArgs e) {
            Command newCommand = groupCommand.Clone();
            if (newCommand.Execute(this)) {
                history.Push(newCommand);
                Subscribe();
                panel1.Refresh();
            }
        }

        private void ungroupSelectedToolStripMenuItem_Click(object sender, EventArgs e) {
            Command newCommand = ungroupCommand.Clone();
            if (newCommand.Execute(this)) {
                history.Push(newCommand);
                Subscribe();
                panel1.Refresh();
            }
        }

        private void stickySelectedToolStripMenuItem_Click(object sender, EventArgs e) {
            Command newCommand = stickyCommand.Clone();
            if (newCommand.Execute(this)) {
                history.Push(newCommand);
                Subscribe();
                panel1.Refresh();
            }
        }

        private void notStickySelectedToolStripMenuItem_Click(object sender, EventArgs e) {
            Command newCommand = notStickyCommand.Clone();
            if (newCommand.Execute(this)) {
                history.Push(newCommand);
                Subscribe();
                panel1.Refresh();
            }
        }

        private void Subscribe() {
            Command newCommand = subscribeCommand.Clone();
            if (newCommand.Execute(this)) {
                history.Push(newCommand);
            }
        }

        private void greenToolStripMenuItem_Click(object sender, EventArgs e) {
            Command newCommand = greenCommand.Clone();
            if (newCommand.Execute(this)) {
                history.Push(newCommand);
                panel1.Refresh();
            }
        }

        private void blackToolStripMenuItem_Click(object sender, EventArgs e) {
            Command newCommand = blackCommand.Clone();
            if (newCommand.Execute(this)) {
                history.Push(newCommand);
                panel1.Refresh();
            }
        }

        private void blueToolStripMenuItem_Click(object sender, EventArgs e) {
            Command newCommand = blueCommand.Clone();
            if (newCommand.Execute(this)) {
                history.Push(newCommand);
                panel1.Refresh();
            }
        }

        private void saveToolStripMenuItem_Click(object sender, EventArgs e) {
            StreamWriter sw = new StreamWriter(str);
            vector.Save(sw);
            sw.Close();
        }

        private void loadToolStripMenuItem_Click(object sender, EventArgs e) {
            StreamReader sr = new StreamReader(str);
            FigureFactory factory = new FirstFactory();
            vector.Load(sr, factory);
            sr.Close();
            panel1.Refresh();
        }

        private void circleToolStripMenuItem_Click(object sender, EventArgs e) {
            state = Figures.Circle;
        }

        private void rectangleToolStripMenuItem_Click(object sender, EventArgs e) {
            state = Figures.Rectangle;
        }

        private void sectionToolStripMenuItem_Click(object sender, EventArgs e) {
            state = Figures.Section;
        }

        private void triangleToolStripMenuItem_Click(object sender, EventArgs e) {
            state = Figures.Triangle;
        }

        private void createSomeObjectsToolStripMenuItem_Click(object sender, EventArgs e) {
            vector.Add(new Circle(329, 123));
            vector.Add(new Circle(401, 123, 20, Color.Blue));
            vector.Add(new Circle(363, 140, 70, Color.Green));
            vector.Add(new Section(364, 185, 60, Color.Green));
            vector.Add(new Section(512, 245, 60, Color.Green));
            vector.Add(new Section(211, 245, 60, Color.Green));
            vector.Add(new Triangle(279, 349, 30, Color.Green));
            vector.Add(new Triangle(439, 349, 30, Color.Green));
            vector.Add(new Rectangle(361, 271, 120, Color.Green));
            panel1.Refresh();
        }

        public void AddObserver(Observer observer) {
            observers.Add(observer);
        }

        public void RemoveObserver(Observer observer) {
            //////////
        }

        public void RemoveObservers() {
            //////////
        }

        public void NotifyAboutMove() {
            //////////
        }

        public void NotifyAboutObstacle() {
            //////////
        }

        public void NotifyAboutChangeInStorage() {
            //////////
        }

        public void NotifyAboutSelectInTree() {
            for (int i = 0; i < observers.Count; i++) {
                observers[i].SelectInTreeUpdate(this);
            }
        }

        public void MoveUpdate(Observable observable) {
            //////////
        }

        public void ObstacleUpdate(Observable observable) {
            //////////
        }

        public void ChangeInStorageUpdate(Observable observable) {
            fromStorage = true;
            treeView1.Nodes.Clear();
            treeView1.Nodes.Add(((MyVector)observable).GetID(), ((MyVector)observable).Classname());
            for (int i = 0; i < ((MyVector)observable).Count(); i++) {
                CreateTree(treeView1.Nodes[0], ((MyVector)observable)[i]);
            }
            treeView1.ExpandAll();
            fromStorage = false;
        }

        public void SelectInTreeUpdate(Observable observable) {
            //////////
        }

        private void CreateTree(TreeNode root, Figure figure) {
            root.Nodes.Add(figure.GetID(), figure.Classname());
            if (figure.IsSelected() && !figure.IsGrouped()) {
                int index = root.Nodes.IndexOfKey(figure.GetID());
                treeView1.SelectedNode = root.Nodes[index];
            }
            if (figure is Group) {
                int index = root.Nodes.IndexOfKey(figure.GetID());
                for (int j = 0; j < ((Group)figure).Count(); j++) {
                    CreateTree(root.Nodes[index], ((Group)figure)[j]);
                }
            }
        }

        private void treeView1_KeyDown(object sender, KeyEventArgs e) {
            e.Handled = true;
        }

        private void treeView1_AfterSelect(object sender, TreeViewEventArgs e) {
            if (fromStorage) {
                return;
            }
            id = Convert.ToInt32(e.Node.Name);
            NotifyAboutSelectInTree();
            panel1.Refresh();
        }

        public int GetID() {
            return id;
        }

        private void undoToolStripMenuItem_Click(object sender, EventArgs e) {
            if (history.Count == 0) {
                return;
            }
            if (history.Peek() is SubscribeCommand) {
                history.Pop().Unexecute();
            }
            history.Pop().Unexecute();
            vector.UpdatePath();
            panel1.Refresh();
        }
    }
}
