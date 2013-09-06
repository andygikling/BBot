#ifndef MAINWINDOW_H
#define MAINWINDOW_H

#include <QGraphicsScene>
#include <QGraphicsPixmapItem>
#include <QGraphicsTextItem>
#include <QPainter>

class MyGraphicsScene : public QGraphicsScene
{
    Q_OBJECT

public:
    explicit MyGraphicsScene();
    ~MyGraphicsScene();

    void SetSize(QSize size);

private slots:
    void Update_();

private:
    void GetImageList_(QString dirName);
    void Display_(int filenameIndex);
    void Display_(QString filename);
    bool HaveFile_(QString filename);
    void DisplayFile_(QString filename);

private:
    QStringList filenameList_;
    int filenameIndex_;
    QGraphicsPixmapItem pixmap_;
    QPainter painter_;
    QSize size_;
};

#endif // MAINWINDOW_H



/*#ifndef MAINWINDOW_H
#define MAINWINDOW_H

#include <QMainWindow>

namespace Ui {
class MainWindow;
}

class MainWindow : public QMainWindow
{
    Q_OBJECT
    
public:
    explicit MainWindow(QWidget *parent = 0);
    ~MainWindow();
    
private:
    Ui::MainWindow *ui;
};

#endif // MAINWINDOW_H
*/
