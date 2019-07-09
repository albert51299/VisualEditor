namespace Editor {
    public interface Observable {
        void AddObserver(Observer observer);
        void RemoveObserver(Observer observer);
        void RemoveObservers();
        void NotifyAboutMove();
        void NotifyAboutObstacle();
        void NotifyAboutChangeInStorage();
        void NotifyAboutSelectInTree();
    }

    public interface Observer {
        void MoveUpdate(Observable observable);
        void ObstacleUpdate(Observable observable);
        void ChangeInStorageUpdate(Observable observable);
        void SelectInTreeUpdate(Observable observable);
    }
}
