namespace Editor {
    public abstract class FigureFactory {
        public abstract Figure CreateFigure(char symbol);
    }

    public class FirstFactory : FigureFactory {
        public override Figure CreateFigure(char symbol) {
            Figure ptr = null;
            switch (symbol) {
                case 'G':
                    ptr = new Group();
                    break;
                case 'C':
                    ptr = new Circle(0, 0);
                    break;
                case 'R':
                    ptr = new Rectangle(0, 0);
                    break;
                case 'S':
                    ptr = new Section(0, 0);
                    break;
                case 'T':
                    ptr = new Triangle(0, 0);
                    break;
            }
            return ptr;
        }
    }
}
