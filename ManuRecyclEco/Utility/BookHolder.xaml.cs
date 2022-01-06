using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace ManuRecyEco.Utility
{
    /// <summary>
    /// Interaction logic for BookHolder.xaml
    /// </summary>
    public partial class BookHolder : UserControl
    {
        public BookHolder()
        {
            InitializeComponent();
        }

        public ImageSource BookImage
        {
            get { return (ImageSource)GetValue(BookImageProperty); }
            set { SetValue(BookImageProperty, value); }
        }

        public static readonly DependencyProperty BookImageProperty =
            DependencyProperty.Register(nameof(BookImage), typeof(ImageSource), typeof(BookHolder),
                new FrameworkPropertyMetadata(null));

        public String BookTitle
        {
            get { return (String)GetValue(BookTitleProperty); }
            set { SetValue(BookTitleProperty, value); }
        }

        public static readonly DependencyProperty BookTitleProperty =
            DependencyProperty.Register(nameof(BookTitle), typeof(String), typeof(BookHolder),
                new FrameworkPropertyMetadata(null));

        public String BookCondition
        {
            get { return (String)GetValue(BookConditionProperty); }
            set { SetValue(BookConditionProperty, value); }
        }

        public static readonly DependencyProperty BookConditionProperty =
            DependencyProperty.Register(nameof(BookCondition), typeof(String), typeof(BookHolder),
                new FrameworkPropertyMetadata(null));

        public String BookPrice
        {
            get { return (String)GetValue(BookPriceProperty); }
            set { SetValue(BookPriceProperty, value); }
        }

        public static readonly DependencyProperty BookPriceProperty =
            DependencyProperty.Register(nameof(BookPrice), typeof(String), typeof(BookHolder),
                new FrameworkPropertyMetadata(null));

        public String TransactionType
        {
            get { return (String)GetValue(TransactionTypeProperty); }
            set { SetValue(TransactionTypeProperty, value); }
        }

        public static readonly DependencyProperty TransactionTypeProperty =
            DependencyProperty.Register(nameof(TransactionType), typeof(String), typeof(BookHolder),
                new FrameworkPropertyMetadata(null));

        public String HyperlinkMessage
        {
            get { return (String)GetValue(HyperlinkMessageProperty); }
            set { SetValue(HyperlinkMessageProperty, value); }
        }

        public static readonly DependencyProperty HyperlinkMessageProperty =
            DependencyProperty.Register(nameof(HyperlinkMessage), typeof(String), typeof(BookHolder),
                new FrameworkPropertyMetadata(null));

        public String CopyId
        {
            get { return (String)GetValue(CopyIdProperty); }
            set { SetValue(CopyIdProperty, value); }
        }

        public static readonly DependencyProperty CopyIdProperty =
            DependencyProperty.Register(nameof(CopyId), typeof(String), typeof(BookHolder),
                new FrameworkPropertyMetadata(null));

        public ICommand DetailPageCommand
        {
            get { return (ICommand)GetValue(DetailPageCommandProperty); }
            set { SetValue(DetailPageCommandProperty, value); }
        }

        public static readonly DependencyProperty DetailPageCommandProperty =
            DependencyProperty.Register(nameof(DetailPageCommand), typeof(ICommand), typeof(BookHolder),
                new FrameworkPropertyMetadata(null));
    }
}
