#include "mainwindow.h"
#include <QApplication>
#include <QGraphicsScene>
#include <QGraphicsView>
#include <QGraphicsPixmapItem>
#include <QWSServer>

int main(int argc, char *argv[])
{
    QApplication a(argc, argv);

    //Remove the mouse pointer from view
    QWSServer::setCursorVisible( false );

    MyGraphicsScene scene;
    QGraphicsView view(&scene);

    //Here we attempt to set the view such that it fills the mini display's
    //screen correctly. The display native resolutioin is 1024 by 600 (annoying)
    //and the BeagleBone Black detects it as 1280 by 768 and just uses that.
    //This causes pictures and the Angstrom desktop to hang off the screen a bit
    //all we need to do is set a scale and it will fill our pictures perfectly!
    view.showFullScreen();
    view.scale(.94,.94); //Ratio found via binary search...
    QSize size(view.size());
    size -= QSize(2,2);
    scene.SetSize(size);

    return a.exec();
}


