#include "mainwindow.h"
//#include <qglobal.h>
//#include <QTime>
#include <QDir>
#include <QGraphicsPixmapItem>
#include <QTimer>
#include <QPainter>
#include <QTextDocument>
#include <QTextBlock>
#include <Qt>

MyGraphicsScene::MyGraphicsScene() :
    QGraphicsScene(),
    filenameList_(), filenameIndex_(0), pixmap_(), painter_(), size_()
{
    addItem(&pixmap_);
    GetImageList_("/home/root/images");
}

MyGraphicsScene::~MyGraphicsScene()
{
}

void MyGraphicsScene::SetSize(QSize size)
{
    size_ = size;
    Display_(0);

    QTimer *timer = new QTimer(this);
    if(connect(timer, SIGNAL(timeout()), this, SLOT(Update_())))
       timer->start(4000);
    else
        Update_();
}

void MyGraphicsScene::Update_()
{
    imageLoopCounter_++;
    if (imageLoopCounter_ == 10)
    {
        GetImageList_("/home/root/images");
        imageLoopCounter_ = 0;
    }

    //Here we'll display the images at random
    int high = filenameList_.length();
    int low = 0;
    int rand = qrand() % ((high + 1) - low) + low;
    //Display_(filenameIndex_ + 1);
    Display_(rand + 1);

    QString logfilename = "/home/root/images/text.txt";
    if(HaveFile_(logfilename))
        DisplayFile_(logfilename);
}

void MyGraphicsScene::Display_(int filenameIndex)
{
    if(filenameIndex >= filenameList_.size())
        filenameIndex = 0;
    if(filenameIndex < filenameList_.size())
    {
        filenameIndex_ = filenameIndex;
        Display_(filenameList_.at(filenameIndex_));
    }
}

void MyGraphicsScene::Display_(QString filename)
{
    QPixmap pm(filename);
    qreal widthScale = (qreal)size_.width() / pm.width();
    qreal heightScale = (qreal)size_.height() / pm.height();

    QPixmap pmf(size_);
    QPainter painter(&pmf);

    if(widthScale < heightScale)
    {
        // fit to width and center the height
        QPixmap pms = pm.scaledToWidth(size_.width());
        int heightDiff = size_.height() - pms.height();
        painter.drawImage(0, heightDiff / 2, pms.toImage());
    }
    else
    {
        // fit to height and center the width
        QPixmap pms = pm.scaledToHeight(size_.height());
        int widthDiff = size_.width() - pms.width();
        painter.drawImage(widthDiff / 2, 0, pms.toImage());
    }

    painter.end();

    pixmap_.setPixmap(pmf);
}

bool MyGraphicsScene::HaveFile_(QString filename)
{
    QFile file(filename);
    return file.exists();
}

void MyGraphicsScene::DisplayFile_(QString filename)
{
    QPixmap pmf(pixmap_.pixmap());
    QPainter painter(&pmf);

    int displayLineSize = 28;
    int displayLineSpacing = 10;
    int displayLineHeight = displayLineSize + displayLineSpacing;
    int displayNumLines = size_.height() / displayLineHeight;
    QFile file(filename);
    file.open(QIODevice::ReadOnly |  QIODevice::Text);
    QByteArray text = file.readAll();
    int fileNumLines = text.count('\n');
    int firstDisplayLine = 0;
    int firstFileLine = 0;
    if(fileNumLines < displayNumLines)
        firstDisplayLine = displayNumLines - fileNumLines;
    else
        firstFileLine = fileNumLines - displayNumLines;

    //Change font and make it bold
    QFont font(painter.font());
    font.setFamily("Courier");
    font.setBold(true);
    font.setPointSize(displayLineSize);
    painter.setFont(font);

    QPen pen(painter.pen());
    pen.setColor(Qt::black);
    pen.setWidth(5);

    QBrush brush(painter.brush());
    brush.setColor(Qt::green);
    brush.setStyle(Qt::SolidPattern);

    QPainterPath pp;
    QList<QByteArray> ss = text.split('\n');

    for(int i = firstDisplayLine, j = firstFileLine; i < displayNumLines; ++i, ++j)
    {
        QString s(ss[j]);
        pp.addText(5, displayLineHeight * (i + 1), font, s);

        //painter.strokePath(pp, pen);
        painter.fillPath(pp, brush);
    }

    file.close();
    painter.end();

    pixmap_.setPixmap(pmf);
}

void MyGraphicsScene::GetImageList_(QString dirName)
{
    filenameList_.clear();

    QDir dir(dirName);
    dir.setFilter(QDir::Files);
    QStringList entries = dir.entryList();
    for(QStringList::ConstIterator entry=entries.begin(); entry!=entries.end(); ++entry)
    {
        QString filename=*entry;
        if((filename.contains(".jpg")) ||
            (filename.contains(".jpeg")) ||
            (filename.contains(".png")) )
            filenameList_.append(dirName + "/" + filename);
    }
}




/*#include "mainwindow.h"
#include "ui_mainwindow.h"

MainWindow::MainWindow(QWidget *parent) :
    QMainWindow(parent),
    ui(new Ui::MainWindow)
{
    ui->setupUi(this);
}

MainWindow::~MainWindow()
{
    delete ui;
}
*/
