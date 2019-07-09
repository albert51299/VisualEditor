using System.Collections.Generic;
using System.Drawing;

namespace Editor {
    public interface Command {
        bool Execute(EditorForm form);
        void Unexecute();
        Command Clone();
    }

    public class AddCommand : Command {
        private MyVector vector;
        private Figure figure;

        public AddCommand() {

        }

        public bool Execute(EditorForm form) { 
            if (form.GetState() != EditorForm.Figures.Nothing) {
                switch (form.GetState()) {
                    case EditorForm.Figures.Circle:
                        figure = new Circle(form.GetPointForAdd().X, form.GetPointForAdd().Y);
                        break;
                    case EditorForm.Figures.Rectangle:
                        figure = new Rectangle(form.GetPointForAdd().X, form.GetPointForAdd().Y);
                        break;
                    case EditorForm.Figures.Section:
                        figure = new Section(form.GetPointForAdd().X, form.GetPointForAdd().Y);
                        break;
                    case EditorForm.Figures.Triangle:
                        figure = new Triangle(form.GetPointForAdd().X, form.GetPointForAdd().Y);
                        break;
                }
                vector = form.GetStorage();
                vector.Add(figure);
                form.ChangeState(EditorForm.Figures.Nothing);
                return true;
            }
            return false;
        }

        public void Unexecute() {
            vector.Remove(figure);
        }

        public Command Clone() {
            return new AddCommand();
        }
    }

    public class RemoveCommand : Command {
        private MyVector vector;
        private List<Figure> shapes = new List<Figure>();

        public RemoveCommand() {
            
        }

        public bool Execute(EditorForm form) {
            bool flag = false;
            vector = form.GetStorage();
            for (int i = vector.Count() - 1; i >= 0; i--) {
                if (vector[i].IsSelected()) {
                    if (vector[i].IsSticky() || vector[i].IsSticked()) {
                        List<Observer> list = vector[i].GetObservers();
                        for (int j = list.Count - 1; j > 0; j--) { 
                            ((Figure)list[j]).RemoveObserver(vector[i]);
                        }
                        if (vector[i].IsSticked()) {
                            vector[i].ChangeSticked(false);
                        }
                    }
                    flag = true;
                    shapes.Add(vector[i]);
                    vector.RemoveAt(i); 
                }
            }
            return flag;
        }

        public void Unexecute() {
            for (int i = 0; i < shapes.Count; i++) {
                vector.Add(shapes[i]);
            }
            for (int i = 0; i < vector.Count(); i++) {
                if (vector[i].IsSticky()) {
                    for (int j = 0; j < vector.Count(); j++) {
                        if (vector[j].IsToStick(vector[i]) && !vector[j].IsSticked() && (i != j)) {
                            vector[j].ChangeSticked(true);
                            vector[j].AddObserver(vector[i]);
                            vector[i].AddObserver(vector[j]);
                        }
                    }
                }
            }
        }

        public Command Clone() {
            return new RemoveCommand();
        }
    }

    public class ScaleCommand : Command {
        private MyVector vector;
        private List<Figure> shapes = new List<Figure>();
        private int delta;

        public ScaleCommand(int delta) {
            this.delta = delta;
        }

        public bool Execute(EditorForm form) {
            bool flag = false;
            vector = form.GetStorage();
            if (delta > 0) {
                for (int i = 0; i < vector.Count(); i++) {
                    if (vector[i].IsSelected() && !vector[i].IsSticked() && !vector[i].IsObstacle(form.GetPanelWidth(), form.GetPanelHeight())) {
                        flag = true;
                        shapes.Add(vector[i]);
                        vector[i].Scale(delta);
                    }
                }
                return flag;
            }
            for (int i = 0; i < vector.Count(); i++) {
                if (vector[i].IsSelected() && !vector[i].IsSticked() && !vector[i].IsTooSmall()) {
                    flag = true;
                    shapes.Add(vector[i]);
                    vector[i].Scale(delta);
                }
            }
            return flag;
        }

        public void Unexecute() {
            for (int i = 0; i < shapes.Count; i++) {
                shapes[i].Scale(-delta);
            }
        }

        public Command Clone() {
            return new ScaleCommand(delta);
        }
    }

    public class MoveCommand : Command {
        private List<Figure> shapes = new List<Figure>();
        private int delta_x;
        private int delta_y;

        public MoveCommand(int delta_x, int delta_y) {
            this.delta_x = delta_x;
            this.delta_y = delta_y;
        }

        public bool Execute(EditorForm form) {
            bool flag = false;
            MyVector vector = form.GetStorage();
            int side = 0;
            if (delta_x < 0) {
                side = 1;
            }
            if (delta_x > 0) {
                side = 3;
            }
            if (delta_y < 0) {
                side = 2;
            }
            if (delta_y > 0) {
                side = 4;
            }
            for (int i = 0; i < vector.Count(); i++) {
                vector[i].ResetObstacles();
                List<Observer> list = vector[i].GetObservers();
                for (int j = 0; j < list.Count; j++) {
                    ((Figure)list[j]).ResetObstacles();
                    ((Figure)list[j]).FillArrayForMove(ref ((Figure)list[j]).GetObstacles(), form.GetPanelWidth(), form.GetPanelHeight());
                    ((Observable)list[j]).NotifyAboutObstacle();
                }
                vector[i].FillArrayForMove(ref vector[i].GetObstacles(), form.GetPanelWidth(), form.GetPanelHeight());
                if (vector[i].IsSelected() && !vector[i].IsSticked() && !vector[i].GetObstacles()[side]) {
                    flag = true;
                    shapes.Add(vector[i]);
                    vector[i].Move(delta_x, delta_y);
                }
            }
            return flag;
        }
   
        public void Unexecute() {
            for (int i = 0; i < shapes.Count; i++) {
                shapes[i].Move(-delta_x, -delta_y); 
            }
        }

        public Command Clone() {
            return new MoveCommand(delta_x, delta_y);
        }
    }

    public class InvertSelectedCommand : Command {
        private List<Figure> shapes = new List<Figure>();

        public InvertSelectedCommand() {

        }

        public bool Execute(EditorForm form) {
            bool flag = false;
            MyVector vector = form.GetStorage();
            for (int i = 0; i < vector.Count(); i++) {
                if (vector[i].IsClickCatchFigure(form.GetFirstPointForSelected())) {
                    flag = true;
                    shapes.Add(vector[i]);
                    vector[i].InvertSelected();
                }
            }
            return flag;
        }

        public void Unexecute() {
            for (int i = 0; i < shapes.Count; i++) {
                shapes[i].InvertSelected();
            }
        }

        public Command Clone() {
            return new InvertSelectedCommand();
        }
    }

    public class ChangeSelectedCommand : Command {
        private List<Figure> shapes = new List<Figure>();
        private bool selected;

        public ChangeSelectedCommand(bool selected) {
            this.selected = selected;
        }

        public bool Execute(EditorForm form) {
            bool flag = false;
            MyVector vector = form.GetStorage();
            for (int i = 0; i < vector.Count(); i++) {
                if (vector[i].IsAreaCatchFigure(form.GetFirstPointForSelected(), form.GetSecondPointForSelected()) && !vector[i].IsSelected()) {
                    flag = true;
                    shapes.Add(vector[i]);
                    vector[i].ChangeSelected(selected);
                }
            }
            return flag;
        }

        public void Unexecute() {
            for (int i = 0; i < shapes.Count; i++) {
                shapes[i].ChangeSelected(!selected);
            }
        }

        public Command Clone() {
            return new ChangeSelectedCommand(selected);
        }
    }

    public class ChangeColorCommand : Command {
        private List<Figure> shapes = new List<Figure>();
        private List<Color> colors = new List<Color>();
        private Color color;

        public ChangeColorCommand(Color color) {
            this.color = color;
        }

        public bool Execute(EditorForm form) {
            bool flag = false;
            MyVector vector = form.GetStorage();
            for (int i = 0; i < vector.Count(); i++) {
                if (vector[i].IsSelected()) { 
                    flag = true;
                    shapes.Add(vector[i]);
                    colors.Add(vector[i].GetColor());
                    vector[i].ChangeColor(color);
                    vector[i].ChangeSelected(false);
                }
            }
            return flag;
        }

        public void Unexecute() {
            for (int i = 0; i < shapes.Count; i++) {
                shapes[i].ChangeColor(colors[i]);
                shapes[i].ChangeSelected(true);
            }
        }

        public Command Clone() {
            return new ChangeColorCommand(color);
        }
    }

    public class GroupCommand : Command {
        private MyVector vector;
        private Group group;
        private List<Figure> not_sticky = new List<Figure>();
        private List<Color> colors = new List<Color>();

        public GroupCommand() {

        }

        public bool Execute(EditorForm form) {
            bool flag = false;
            vector = form.GetStorage();
            Group newGroup = new Group();
            newGroup.ChangeSelected(true);
            bool stickyGroup = false;
            for (int i = 0; i < vector.Count(); i++) { 
                if (vector[i].IsSticky() && vector[i].IsSelected()) {
                    stickyGroup = true;
                    break;
                }
            }
            for (int i = vector.Count() - 1; i >= 0; i--) {
                if (vector[i].IsSelected()) {
                    vector[i].ChangeGrouped(true);
                    if (stickyGroup && !vector[i].IsSticky()) {
                        not_sticky.Add(vector[i]);
                    }
                    if (vector[i].IsSticky() || vector[i].IsSticked()) {
                        List<Observer> list = vector[i].GetObservers();
                        for (int j = list.Count - 1; j > 0; j--) { 
                            ((Figure)list[j]).RemoveObserver(vector[i]);
                        }
                    }
                    colors.Add(vector[i].GetColor());
                    vector[i].ChangeColor(Color.Green);
                    newGroup.Add(vector[i]);
                    vector.RemoveAt(i); 
                }
            }
            if (newGroup.Count() != 0) {
                flag = true;
                if (stickyGroup) {
                    newGroup.ChangeSticky(true);
                }
                vector.Add(newGroup);
                group = newGroup;
            }
            return flag;
        }

        public void Unexecute() {
            for (int i = 0; i < not_sticky.Count; i++) {
                not_sticky[i].ChangeSticky(false);
            }
            vector.Remove(group);
            for (int i = 0; i < group.Count(); i++) {
                group[i].ChangeColor(colors[i]);
                group[i].ChangeGrouped(false);
                vector.Add(group[i]);
            }
            for (int i = 0; i < vector.Count(); i++) {
                if (vector[i].IsSticky()) {
                    for (int j = 0; j < vector.Count(); j++) {
                        if (vector[j].IsToStick(vector[i]) && !vector[j].IsSticked() && (i != j)) {
                            vector[j].ChangeSticked(true);
                            vector[j].AddObserver(vector[i]);
                            vector[i].AddObserver(vector[j]);
                        }
                    }
                }
            }
        }

        public Command Clone() {
            return new GroupCommand();
        }
    }

    public class UngroupCommand : Command {
        private MyVector vector;
        private List<Group> groups = new List<Group>();

        public UngroupCommand() {
            
        }

        public bool Execute(EditorForm form) {
            bool flag = false;
            vector = form.GetStorage();
            for (int i = vector.Count() - 1; i >= 0; i--) {
                if ((vector[i] is Group) && vector[i].IsSelected()) {
                    flag = true;
                    groups.Add((Group)vector[i]);
                    if (vector[i].IsSticky() || vector[i].IsSticked()) {
                        List<Observer> list = vector[i].GetObservers();
                        for (int j = 1; j < list.Count; j++) {
                            ((Figure)list[j]).RemoveObserver(vector[i]);
                        }
                    }
                    if (vector[i].IsSticked()) {
                        vector[i].ChangeSticked(false);
                    }
                    for (int j = 0; j < ((Group)vector[i]).Count(); j++) {
                        ((Group)vector[i])[j].ChangeGrouped(false);
                        if (((Group)vector[i])[j] is Group) {
                            for (int k = 0; k < ((Group)((Group)vector[i])[j]).Count(); k++) {
                                ((Group)((Group)vector[i])[j])[k].ChangeGrouped(true);
                            }
                        }
                        vector.Add(((Group)vector[i])[j]);
                    }
                    vector.RemoveAt(i);
                }
            }
            return flag;
        }

        public void Unexecute() {
            for (int i = 0; i < groups.Count; i++) {
                for (int j = 0; j < groups[i].Count(); j++) {
                    for (int k = vector.Count() - 1; k >= 0; k--) {
                        if (vector[k] == groups[i][j]) {
                            vector[k].ChangeGrouped(true);
                            vector.RemoveAt(k);
                        }
                    }
                }
                vector.Add(groups[i]);
            }
            for (int i = 0; i < vector.Count(); i++) {
                if (vector[i].IsSticky()) {
                    for (int j = 0; j < vector.Count(); j++) {
                        if (vector[j].IsToStick(vector[i]) && !vector[j].IsSticked() && (i != j)) {
                            vector[j].ChangeSticked(true);
                            vector[j].AddObserver(vector[i]);
                            vector[i].AddObserver(vector[j]);
                        }
                    }
                }
            }
        }

        public Command Clone() {
            return new UngroupCommand();
        }
    }

    public class StickyCommand : Command {
        private List<Figure> shapes = new List<Figure>();

        public StickyCommand() {

        }

        public bool Execute(EditorForm form) {
            bool flag = false;
            MyVector vector = form.GetStorage();
            for (int i = 0; i < vector.Count(); i++) {
                if (vector[i].IsSelected() && !vector[i].IsSticky()) {
                    flag = true;
                    shapes.Add(vector[i]);
                    vector[i].ChangeSticky(true);
                    vector[i].ChangeSelected(false);
                }
            }
            return flag;
        }

        public void Unexecute() {
            for (int i = 0; i < shapes.Count; i++) {
                shapes[i].ChangeSticky(false);
                shapes[i].ChangeSelected(true);
            }
        }

        public Command Clone() {
            return new StickyCommand();
        }
    }

    public class NotStickyCommand : Command {
        private List<Figure> shapes = new List<Figure>();
        private List<List<Observer>> listOfObs = new List<List<Observer>>();

        public NotStickyCommand() {

        }

        public bool Execute(EditorForm form) {
            bool flag = false;
            MyVector vector = form.GetStorage();
            for (int i = 0; i < vector.Count(); i++) {
                if (vector[i].IsSelected() && vector[i].IsSticky()) {
                    flag = true;
                    shapes.Add(vector[i]);                           
                    listOfObs.Add(new List<Observer>());             
                    List<Observer> list = vector[i].GetObservers();  
                    for (int j = list.Count - 1; j > 0; j--) {
                        listOfObs[shapes.Count - 1].Add(list[j]);
                        ((Figure)list[j]).RemoveObserver(vector[i]); 
                        ((Figure)list[j]).ChangeSticked(false);
                        list.RemoveAt(j);            
                    }
                    vector[i].ChangeSticky(false);
                    vector[i].ChangeSelected(false);
                }
            }
            return flag;
        }

        public void Unexecute() {
            for (int i = 0; i < shapes.Count; i++) {
                for (int j = 0; j < listOfObs[i].Count; j++) {
                    shapes[i].AddObserver(listOfObs[i][j]);
                    ((Figure)listOfObs[i][j]).AddObserver(shapes[i]);
                    ((Figure)listOfObs[i][j]).ChangeSticked(true);
                }
                shapes[i].ChangeSticky(true);
                shapes[i].ChangeSelected(true);
            }
        }

        public Command Clone() {
            return new NotStickyCommand();
        }
    }

    public class SubscribeCommand : Command {
        private List<Figure> sticked_shapes = new List<Figure>();

        public SubscribeCommand() {

        }

        public bool Execute(EditorForm form) {
            bool flag = false;
            MyVector vector = form.GetStorage();
            for (int i = 0; i < vector.Count(); i++) {
                if (vector[i].IsSticky()) {
                    for (int j = 0; j < vector.Count(); j++) {
                        if (vector[j].IsToStick(vector[i]) && !vector[j].IsSticked() && (i != j)) {
                            flag = true;
                            sticked_shapes.Add(vector[j]);
                            vector[j].ChangeSticked(true);
                            vector[i].AddObserver(vector[j]);
                            vector[j].AddObserver(vector[i]);
                        }
                    }
                }
            }
            return flag;
        }

        public void Unexecute() {
            for (int i = 0; i < sticked_shapes.Count; i++) {
                sticked_shapes[i].ChangeSticked(false);
                List<Observer> list = sticked_shapes[i].GetObservers();
                for (int j = list.Count - 1; j > 0; j--) {
                    ((Figure)list[j]).RemoveObserver(sticked_shapes[i]);
                    list.RemoveAt(j);
                }
            }
        }

        public Command Clone() {
            return new SubscribeCommand();
        }
    }
}
