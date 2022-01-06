using ManuRecyEco.Models;
using ManuRecyEco.Utility;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ManuRecyEco
{
    public static class MyExtensions
    {
        public static void AddCourseBook(this List<AcademicCourseBook> list, int courseId, int bookId, List<Book> bookList)
        {
            if(bookList.Where(b => b.Id == bookId).Any())
            {
                list.Add(new AcademicCourseBook() { BookId = bookId, AcademicCourseId = courseId });
            }
        }
    }

    public class ApplicationDbContext: DbContext
    {
        public DbSet<User> Users { get; set; }
        public DbSet<Token> Tokens { get; set; }
        public DbSet<City> Cities { get; set; }
        public DbSet<SchoolLevel> SchoolLevels { get; set; }
        public DbSet<School> Schools { get; set; }
        public DbSet<Book> Books { get; set; }
        public DbSet<BookCopy> BookCopies { get; set; }
        public DbSet<AcademicCourse> AcademicCourses { get; set; }
        public DbSet<AcademicProgram> AcademicPrograms { get; set; }
        public DbSet<AcademicCourseBook> AcademicCoursesBooks { get; set; }
        public DbSet<AcademicCourseProgram> AcademicCoursesPrograms { get; set; }
        public DbSet<AcademicCourseUser> AcademicCoursesUsers { get; set; }
        public DbSet<BookUser> BooksUsers { get; set; }
        public DbSet<ImageCopy> ImageCopies { get; set; }

        public void SetUserLastLogin(User User)
        {
            User.LastLogin = DateTime.Now.ToString("s", CultureInfo.InvariantCulture);
            _ = Users.Update(User);
            _ = SaveChanges();
        }

        public void AddToken(User User, string Token)
        {
            _ = Tokens.Add(new Token
            {
                User = User,
                RandomString = Token,
                SentTime = DateTime.Now.ToString("s", CultureInfo.InvariantCulture),
            });
            _ = SaveChanges();
        }

        public List<BookCopy> GetProgramListExemplaires(List<AcademicProgram> ProgramList)
        {
            return ProgramList.
                Join(AcademicCoursesPrograms,
                    ap => ap.Id,
                    acp => acp.AcademicProgramId,
                    (ap, acp) => acp).
                        Join(AcademicCoursesBooks,
                            acp => acp.AcademicCourseId,
                            acb => acb.AcademicCourseId,
                            (acp, acb) => acb).
                                Join(BookCopies,
                                    acb => acb.BookId,
                                    bc => bc.BookId,
                                    (acb, bc) => bc).Distinct().ToList();

        }

        public List<BookCopy> GetProgramExemplaires(AcademicProgram Program)
        {
            List<AcademicProgram> program = new List<AcademicProgram>() { Program };
            return GetProgramListExemplaires(program);
        }

        public List<BookCopy> GetCourseListExemplaires(List<AcademicCourse> CourseList)
        {
            return CourseList.
                Join(AcademicCoursesBooks,
                    cl => cl.Id,
                    acb => acb.AcademicCourseId,
                    (cl, acb) => acb).
                        Join(BookCopies,
                            acb => acb.BookId,
                            bc => bc.BookId,
                            (acb, bc) => bc).Distinct().ToList();
        }

        public List<BookCopy> GetCourseExemplaires(AcademicCourse Course)
        {
            List<AcademicCourse> course = new List<AcademicCourse>() { Course };
            return GetCourseListExemplaires(course);
        }

        public List<BookCopy> GetBookListExemplaires(List<Book> BookList)
        {
            return BookList.
                Join(BookCopies,
                    bl => bl.Id,
                    bc => bc.BookId,
                    (bl, bc) => bc).Distinct().ToList();
        }

        public List<BookCopy> GetBookExemplaires(Book Book)
        {
            List<Book> book = new List<Book>() { Book };
            return GetBookListExemplaires(book);
        }

        public int GetUserBookCopyCount(User User)
        {
            return BookCopies.
                Where(bc => bc.UserId == User.Id).ToList().Count;
        }

        public List<Book> GetUserWantedBooks(User User)
        {
            return BooksUsers.
                Where(bu => bu.UserId == User.Id).
                    Select(bu => bu.Book).ToList();
        }

        // retourne la liste de programmes à l'UQAM
        public List<AcademicProgram> GetUqamProgramListWithDefault()
        {
            School Uqam = Schools.Where(s => s.Name == "Université du Québec à Montréal").Single();

            return AcademicPrograms.
                Where(p => p.School.Equals(Uqam)).
                    OrderBy(p => p.Name).ToList();
        }

        public List<AcademicProgram> GetUqamProgramListExceptDefault()
        {
            School Uqam = Schools.Where(s => s.Name == "Université du Québec à Montréal").Single();

            return AcademicPrograms.
                Where(p => p.School.Equals(Uqam)).
                    Where(ap => ap.Id != -1).
                        OrderBy(p => p.Name).ToList();
        }

        // retourne la liste de cours pour une liste de programmes donnée
        public List<AcademicCourse> GetCourses(List<AcademicProgram> ProgramList)
        {
            return ProgramList.
                Join(AcademicCoursesPrograms,
                    p => p.Id,
                    cp => cp.AcademicProgramId,
                    (p, cp) => cp.AcademicCourse).
                        Distinct().OrderBy(c => c.Acronym).ToList();
        }

        // retourne la liste de cours pour un programme donné
        public List<AcademicCourse> GetCourses(AcademicProgram Program)
        {
            return AcademicCoursesPrograms.
                Where(cp => cp.AcademicProgram.Equals(Program)).
                    Select(cp => cp.AcademicCourse).
                        OrderBy(c => c.Acronym).ToList();
        }

        // retourne la liste de cours pour un userId donné
        public List<AcademicCourse> GetUserCourses(int UserId)
        {
            return AcademicCoursesUsers.
                Where(cu => cu.UserId == UserId).
                    Select(cu => cu.AcademicCourse).
                        OrderBy(c => c.Acronym).ToList();
        }

        // retourne la liste de cours qui ont au moins un livre avec un 
        // exemplaire dans la base de donnée excluant les exemplaires
        // publiés par le user passé en paramètre.
        public List<AcademicCourse> GetCoursesWithBookCopies(int UserId)
        {
            return AcademicCoursesBooks.
                Join(BookCopies,
                    acb => acb.BookId,
                    bc => bc.BookId,
                    (b, bc) => bc).
                        Where(bc => bc.UserId != UserId).
                            Join(AcademicCoursesBooks,
                            bc => bc.BookId,
                            acb => acb.BookId,
                            (bc, acb) => acb.AcademicCourse).
                                OrderBy(c => c.Acronym).ToList();
        }

        // retourne la liste de cours d'un user pour lesquel il existe au moins un
        // exemplaire dans la BD non-publié par ce même user
        public List<AcademicCourse> GetUserCoursesWithBookCopies(int UserId)
        {
            return GetUserCourses(UserId).
                        Intersect(GetCoursesWithBookCopies(UserId)).
                            OrderBy(c => c.Acronym).ToList();
        }

        // retourne la liste de livre pour une liste de cours donnée
        public List<Book> GetBooks(List<AcademicCourse> CourseList)
        {
            return CourseList.
                Join(AcademicCoursesBooks,
                    c => c.Id,
                    cb => cb.AcademicCourseId,
                    (c, cb) => cb.Book).
                        Distinct().OrderBy(b => b.Title).ToList();
        }

        // retourne la liste de livres pour un cours donné
        public List<Book> GetBooks(AcademicCourse Course)
        {
            return AcademicCoursesBooks.
                Where(cb => cb.AcademicCourse.Equals(Course)).
                    Select(cb => cb.Book).
                        OrderBy(b => b.Title).ToList();
        }

        // retourne la liste de livres d'une liste de cours pour lesquels il existe au moins 1 exemplaire
        // non-publié par le user passé en paramètre.
        public List<Book> GetCourseListBooksWithBookCopies(List<AcademicCourse> CourseList, int UserId)
        {
            return CourseList.Join(AcademicCoursesBooks,
                ac => ac.Id,
                acb => acb.AcademicCourseId,
                (ac, acb) => acb.Book).
                    Join(BookCopies,
                        b => b.Id,
                        bc => bc.BookId,
                        (b, bc) => bc).
                            Where(bc => bc.UserId != UserId).
                                Select(bc => bc.Book).
                                    Distinct().OrderBy(b => b.Title).ToList();
        }

        // retourne la liste de livres d'un cours pour lesquels il existe au moins 1 exemplaire
        // non-publié par le user passé en paramètre.
        public List<Book> GetCourseBooksWithBookCopies(AcademicCourse Course, int UserId)
        {
            return AcademicCoursesBooks.
                Where(acb => acb.AcademicCourse.Equals(Course)).
                    Select(acb => acb.Book).
                        Join(BookCopies,
                            b => b.Id,
                            bc => bc.BookId,
                            (b, bc) => bc).
                                Where(bc => bc.UserId != UserId).
                                    Select(bc => bc.Book).
                                        Distinct().OrderBy(b => b.Title).ToList();
        }

        // Exclut les exemplaires du user or ordonne les résultats
        private List<BookCopy> ExcludeUserAndOrderQuery(IEnumerable<BookCopy> query, int UserId)
        {
            return query.OrderBy(bc => bc.Price).
                        OrderByDescending(bc => bc.Condition).
                        OrderBy(bc => bc.Book.Title).ToList();
        }

        // retourne tous les exemplaires des livres d'une liste de cours donnée
        // excluant ceux publiés par le user passé en paramètre
        public List<BookCopy> GetCourseListBooksCopies(List<AcademicCourse> CourseList, int UserId)
        {
            IEnumerable<BookCopy> courseListBookCopies = CourseList.Join(AcademicCoursesBooks,
                                                            c => c.Id,
                                                            cb => cb.AcademicCourseId,
                                                            (c, cb) => cb.Book).
                                                                Join(BookCopies,
                                                                    b => b.Id,
                                                                    bc => bc.BookId,
                                                                    (b, bc) => bc).Distinct();

            return ExcludeUserAndOrderQuery(courseListBookCopies, UserId);
        }

        // retourne tous les exemplaires des livres d'un cours donnée
        // excluant ceux publiés par le user passé en paramètre
        public List<BookCopy> GetCourseBooksCopies(AcademicCourse Course, int UserId)
        {
            IEnumerable<BookCopy> courseBookCopies = AcademicCoursesBooks.
                                                        Where(acb => acb.AcademicCourse.Equals(Course)).
                                                            Select(acb => acb.Book).
                                                                    Join(BookCopies,
                                                                        b => b.Id,
                                                                        bc => bc.BookId,
                                                                        (b, bc) => bc).Distinct();

            return ExcludeUserAndOrderQuery(courseBookCopies, UserId);
        }

        // retourne tous les exemplaires d'un livre donnée
        // excluant ceux publiés par le user passé en paramètre
        public List<BookCopy> GetBookCopies(Book Book, int UserId)
        {
            IEnumerable<BookCopy> bookCopies = BookCopies.Where(bc => bc.Book.Equals(Book));

            return ExcludeUserAndOrderQuery(bookCopies, UserId);
        }



        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Foreign key SchoolLevelId
            modelBuilder.Entity<School>(school => school
                                                .HasOne(x => x.SchoolLevel)
                                                .WithMany(x => x.Schools)
                                                .HasForeignKey(x => x.SchoolLevelId)
                                                .IsRequired());

            // Foreign key SchoolId
            modelBuilder.Entity<AcademicProgram>(ap => ap
                                                .HasOne(x => x.School)
                                                .WithMany(x => x.AcademicPrograms)
                                                .HasForeignKey(x => x.SchoolId)
                                                .IsRequired());

            // Foreign key BookId
            modelBuilder.Entity<BookCopy>(bc => bc
                                                .HasOne(x => x.Book)
                                                .WithMany(x => x.BookCopies)
                                                .HasForeignKey(x => x.BookId)
                                                .IsRequired());

            // Foreign key BookId
            modelBuilder.Entity<BookCopy>(bc => bc
                                                .HasOne(x => x.User)
                                                .WithMany(x => x.BookCopies)
                                                .HasForeignKey(x => x.UserId)
                                                .IsRequired());


            // Foreign key CityId
            modelBuilder.Entity<User>(user => user
                                                .HasOne(x => x.City)
                                                .WithMany(x => x.Users)
                                                .HasForeignKey(x => x.CityId)
                                                .IsRequired());

            // Foreign key UserId
            modelBuilder.Entity<Token>(token => token
                                                .HasOne(x => x.User)
                                                .WithMany(x => x.Tokens)
                                                .HasForeignKey(x => x.UserId)
                                                .IsRequired());

            // Foreign key ProgramId
            modelBuilder.Entity<User>(user => user
                                                .HasOne(x => x.AcademicProgram)
                                                .WithMany(x => x.ProgramUsers)
                                                .HasForeignKey(x => x.AcademicProgramId));

            // Default false
            modelBuilder.Entity<User>().Property(x => x.IsActive).HasDefaultValue(0);

            // clés primaires tables intermédiaires
            modelBuilder.Entity<AcademicCourseProgram>().HasKey(x => new { x.AcademicCourseId, x.AcademicProgramId });
            modelBuilder.Entity<AcademicCourseBook>().HasKey(x => new { x.AcademicCourseId, x.BookId });
            modelBuilder.Entity<AcademicCourseUser>().HasKey(x => new { x.AcademicCourseId, x.UserId });
            modelBuilder.Entity<BookUser>().HasKey(x => new { x.BookId, x.UserId });

            // Création de la BD de départ

            // true: random mode, false: hardcoded data
            bool RANDOM = true;

            // nombre de livres voulus pour générer les exemplaires
            // Pour tous les livres mettre 93
            int NUM_BOOKS = 40;

            // nombre d'exemplaires voulu
            int NUM_COPIES = 110;

            // nombre d'exemplaires maximum par livre
            int NB_MAX_COPIES = 4;

            // nombre d'exemplaires maximum du
            // même livre avec le même état
            int NB_MAX_CONDITION = 2;

            // nombre d'exemplaires maximum du
            // même livre avec le même prix
            int NB_MAX_PRICE = 1;

            Random rnd = new Random();

            #region villes

            List<City> cities = new List<City>();

            for (int i = 1; i <= cityNames.Count; i++)
            {
                cities.Add(new City { Id = i, Name = cityNames[i - 1] });
            }

            modelBuilder.Entity<City>().HasData(cities);

            #endregion

            #region niveaux

            // insert des types d'institutions           
            modelBuilder.Entity<SchoolLevel>().HasData(new SchoolLevel { Id = 1, Name = "Universitaire" });
            modelBuilder.Entity<SchoolLevel>().HasData(new SchoolLevel { Id = 2, Name = "Collégial" });

            #endregion

            #region institutions

            School Uqam = new School { Id = 1, Name = "Université du Québec à Montréal", SchoolLevelId = 1, Address = "405 Rue Sainte-Catherine Est, Montréal" };

            modelBuilder.Entity<School>().HasData(Uqam);
            modelBuilder.Entity<School>().HasData(new School { Id = 2, Name = "Université de Montréal", SchoolLevelId = 1, Address = "2900 Boulevard Edouard-Montpetit, Montréal" });
            modelBuilder.Entity<School>().HasData(new School { Id = 3, Name = "Cégep du Vieux Montréal", SchoolLevelId = 2, Address = "255 Rue Ontario E, Montréal" });

            #endregion

            #region programmes

            List<AcademicProgram> programs = new List<AcademicProgram>();

            programs.Add(new AcademicProgram { Id = 1, Name = "Baccalauréat en informatique et génie logiciel", SchoolId = Uqam.Id });
            programs.Add(new AcademicProgram { Id = 2, Name = "Certificat en informatique", SchoolId = Uqam.Id });
            programs.Add(new AcademicProgram { Id = 3, Name = "Certificat avancé en informatique", SchoolId = Uqam.Id });
            programs.Add(new AcademicProgram { Id = 4, Name = "Maîtrise en informatique", SchoolId = Uqam.Id });

            modelBuilder.Entity<AcademicProgram>().HasData(programs);

            #endregion

            #region cours

            List<AcademicCourse> courses = new List<AcademicCourse>();

            // baccalauréat en informatique
            courses.Add(new AcademicCourse { Id = 1, Name = "Utilisation et administration des systèmes informatiques", Acronym = "INF1070" });
            courses.Add(new AcademicCourse { Id = 2, Name = "Programmation I", Acronym = "INF1120" });
            courses.Add(new AcademicCourse { Id = 3, Name = "Outils et pratiques de développement logiciel", Acronym = "INF2050" });
            courses.Add(new AcademicCourse { Id = 4, Name = "Structures de données et algorithmes", Acronym = "INF3105" });
            courses.Add(new AcademicCourse { Id = 5, Name = "Construction et maintenance de logiciels", Acronym = "INF3135" });
            courses.Add(new AcademicCourse { Id = 6, Name = "Mathématiques pour l'informatique", Acronym = "INF1132" });
            courses.Add(new AcademicCourse { Id = 7, Name = "Programmation fonctionnelle et logique", Acronym = "INF6120" });
            courses.Add(new AcademicCourse { Id = 8, Name = "Projet d'analyse et de modélisation", Acronym = "INM5151" });
            courses.Add(new AcademicCourse { Id = 9, Name = "Informatique et société", Acronym = "INM6000" });
            courses.Add(new AcademicCourse { Id = 15, Name = "Programmation Web avancée", Acronym = "INF5190" });
            courses.Add(new AcademicCourse { Id = 10, Name = "Génie logiciel: conduite de projets informatiques", Acronym = "INF6150" });
            courses.Add(new AcademicCourse { Id = 16, Name = "Génie logiciel: conception", Acronym = "INF5153" });
            courses.Add(new AcademicCourse { Id = 17, Name = "Intelligence Artificielle", Acronym = "INF4230" });
            courses.Add(new AcademicCourse { Id = 21, Name = "Téléinformatique", Acronym = "INF3271" });
            courses.Add(new AcademicCourse { Id = 22, Name = "Introduction à la programmation web", Acronym = "INF3190" });
            courses.Add(new AcademicCourse { Id = 23, Name = "Principes des systèmes d'exploitation", Acronym = "INF3173" });
            courses.Add(new AcademicCourse { Id = 24, Name = "Organisation des ordinateurs et assembleur", Acronym = "INF2171" });
            courses.Add(new AcademicCourse { Id = 25, Name = "Bases de données", Acronym = "INF3080" });
            courses.Add(new AcademicCourse { Id = 26, Name = "Programmation II", Acronym = "INF2120" });
            courses.Add(new AcademicCourse { Id = 27, Name = "Création de langages informatiques", Acronym = "INF600E" });

            // maîtrise en informatique
            courses.Add(new AcademicCourse { Id = 11, Name = "Conception et analyse des algorithmes", Acronym = "INF7440" });
            courses.Add(new AcademicCourse { Id = 12, Name = "Théorie des langages et des automates", Acronym = "INF7541" });
            courses.Add(new AcademicCourse { Id = 18, Name = "Initiation à la recherche en informatique", Acronym = "INF8000" });
            courses.Add(new AcademicCourse { Id = 19, Name = "Sécurité des systèmes informatiques", Acronym = "INF8750" });
            courses.Add(new AcademicCourse { Id = 14, Name = "Fondements de l'intelligence artificielle", Acronym = "INF8790" });
            courses.Add(new AcademicCourse { Id = 20, Name = "Principes de simulation", Acronym = "MAT8780" });
            courses.Add(new AcademicCourse { Id = 13, Name = "Traitement automatique du langage naturel", Acronym = "INF7546" });

            modelBuilder.Entity<AcademicCourse>().HasData(courses);

            #endregion

            #region cours-programmes

            List<AcademicCourseProgram> courseProgram = new List<AcademicCourseProgram>();

            // baccalauréat en informatique
            courseProgram.Add(new AcademicCourseProgram { AcademicProgramId = 1, AcademicCourseId = 1 });
            courseProgram.Add(new AcademicCourseProgram { AcademicProgramId = 1, AcademicCourseId = 2 });
            courseProgram.Add(new AcademicCourseProgram { AcademicProgramId = 1, AcademicCourseId = 3 });
            courseProgram.Add(new AcademicCourseProgram { AcademicProgramId = 1, AcademicCourseId = 4 });
            courseProgram.Add(new AcademicCourseProgram { AcademicProgramId = 1, AcademicCourseId = 5 });
            courseProgram.Add(new AcademicCourseProgram { AcademicProgramId = 1, AcademicCourseId = 6 });
            courseProgram.Add(new AcademicCourseProgram { AcademicProgramId = 1, AcademicCourseId = 7 });
            courseProgram.Add(new AcademicCourseProgram { AcademicProgramId = 1, AcademicCourseId = 8 });
            courseProgram.Add(new AcademicCourseProgram { AcademicProgramId = 1, AcademicCourseId = 9 });
            courseProgram.Add(new AcademicCourseProgram { AcademicProgramId = 1, AcademicCourseId = 10 });
            courseProgram.Add(new AcademicCourseProgram { AcademicProgramId = 1, AcademicCourseId = 15 });
            courseProgram.Add(new AcademicCourseProgram { AcademicProgramId = 1, AcademicCourseId = 16 });
            courseProgram.Add(new AcademicCourseProgram { AcademicProgramId = 1, AcademicCourseId = 17 });
            courseProgram.Add(new AcademicCourseProgram { AcademicProgramId = 1, AcademicCourseId = 21 });
            courseProgram.Add(new AcademicCourseProgram { AcademicProgramId = 1, AcademicCourseId = 22 });
            courseProgram.Add(new AcademicCourseProgram { AcademicProgramId = 1, AcademicCourseId = 23 });
            courseProgram.Add(new AcademicCourseProgram { AcademicProgramId = 1, AcademicCourseId = 24 });
            courseProgram.Add(new AcademicCourseProgram { AcademicProgramId = 1, AcademicCourseId = 25 });
            courseProgram.Add(new AcademicCourseProgram { AcademicProgramId = 1, AcademicCourseId = 26 });
            courseProgram.Add(new AcademicCourseProgram { AcademicProgramId = 1, AcademicCourseId = 27 });
            
            // certificat en informatique
            courseProgram.Add(new AcademicCourseProgram { AcademicProgramId = 2, AcademicCourseId = 1 });
            courseProgram.Add(new AcademicCourseProgram { AcademicProgramId = 2, AcademicCourseId = 2 });
            courseProgram.Add(new AcademicCourseProgram { AcademicProgramId = 2, AcademicCourseId = 3 });
            courseProgram.Add(new AcademicCourseProgram { AcademicProgramId = 2, AcademicCourseId = 4 });
            courseProgram.Add(new AcademicCourseProgram { AcademicProgramId = 2, AcademicCourseId = 5 });
            courseProgram.Add(new AcademicCourseProgram { AcademicProgramId = 2, AcademicCourseId = 6 });
            courseProgram.Add(new AcademicCourseProgram { AcademicProgramId = 2, AcademicCourseId = 7 });
            courseProgram.Add(new AcademicCourseProgram { AcademicProgramId = 2, AcademicCourseId = 21 });
            courseProgram.Add(new AcademicCourseProgram { AcademicProgramId = 2, AcademicCourseId = 22 });
            courseProgram.Add(new AcademicCourseProgram { AcademicProgramId = 2, AcademicCourseId = 24 });
            courseProgram.Add(new AcademicCourseProgram { AcademicProgramId = 2, AcademicCourseId = 26 });

            // certificat avancé en informatique
            courseProgram.Add(new AcademicCourseProgram { AcademicProgramId = 3, AcademicCourseId = 4 });
            courseProgram.Add(new AcademicCourseProgram { AcademicProgramId = 3, AcademicCourseId = 5 });
            courseProgram.Add(new AcademicCourseProgram { AcademicProgramId = 3, AcademicCourseId = 6 });
            courseProgram.Add(new AcademicCourseProgram { AcademicProgramId = 3, AcademicCourseId = 7 });
            courseProgram.Add(new AcademicCourseProgram { AcademicProgramId = 3, AcademicCourseId = 15 });
            courseProgram.Add(new AcademicCourseProgram { AcademicProgramId = 3, AcademicCourseId = 16 });
            courseProgram.Add(new AcademicCourseProgram { AcademicProgramId = 3, AcademicCourseId = 23 });
            courseProgram.Add(new AcademicCourseProgram { AcademicProgramId = 3, AcademicCourseId = 25 });

            // maîtrise en informatique
            courseProgram.Add(new AcademicCourseProgram { AcademicProgramId = 4, AcademicCourseId = 11 });
            courseProgram.Add(new AcademicCourseProgram { AcademicProgramId = 4, AcademicCourseId = 12 });
            courseProgram.Add(new AcademicCourseProgram { AcademicProgramId = 4, AcademicCourseId = 13 });
            courseProgram.Add(new AcademicCourseProgram { AcademicProgramId = 4, AcademicCourseId = 14 });
            courseProgram.Add(new AcademicCourseProgram { AcademicProgramId = 4, AcademicCourseId = 18 });
            courseProgram.Add(new AcademicCourseProgram { AcademicProgramId = 4, AcademicCourseId = 19 });
            courseProgram.Add(new AcademicCourseProgram { AcademicProgramId = 4, AcademicCourseId = 20 });

            modelBuilder.Entity<AcademicCourseProgram>().HasData(courseProgram);

            #endregion

            #region livres-images

            // path pour les images de livres (e.g. à partir de l'exécutable dans bin/debug/etc)
            string path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"..\..\..\..\livres\");

            List<Book> books = new List<Book>();
            List<ImageCopy> images = new List<ImageCopy>();

            #region ajouts livres

            ImageCopy imgCpy = new ImageCopy(); imgCpy.Id = 1; imgCpy.ImageToBlob(Path.Combine(path, "1.jpg"), ".jpg"); images.Add(imgCpy);
            Book book = new Book
            {
                Id = 1,
                ImageCopyId = 1,
                Title = "Algorithmique : raisonner pour concevoir",
                Author = "Christophe Haro",
                ISBN = "9782409024412",
                Publisher = "Parchemins",
                NumPages = 678,
                Year = 2012,
                ReferencePrice = 69.99
            };
            books.Add(book);

            imgCpy = new ImageCopy(); imgCpy.Id = 2; imgCpy.ImageToBlob(Path.Combine(path, "2.jpg"), ".jpg"); images.Add(imgCpy);
            book = new Book
            {
                Id = 2,
                ImageCopyId = 2,
                Title = "Python 3 : les fondamentaux du langage",
                Author = "Sébastien Chazallet",
                ISBN = "9782409020964",
                Publisher = "Parchemins",
                NumPages = 384,
                Year = 2006,
                ReferencePrice = 45.50
            };
            books.Add(book);

            imgCpy = new ImageCopy(); imgCpy.Id = 3; imgCpy.ImageToBlob(Path.Combine(path, "3.jpg"), ".jpg"); images.Add(imgCpy);
            book = new Book
            {
                Id = 3,
                ImageCopyId = 3,
                Title = "Programmer en C++ moderne : de C++ 11 à C++ 20",
                Author = "Claude Delannoy",
                ISBN = "9782212678956",
                Publisher = "InfoSphère",
                NumPages = 783,
                Year = 2017,
                ReferencePrice = 89.99
            };
            books.Add(book);

            imgCpy = new ImageCopy(); imgCpy.Id = 4; imgCpy.ImageToBlob(Path.Combine(path, "4.jpg"), ".jpg"); images.Add(imgCpy);
            book = new Book
            {
                Id = 4,
                ImageCopyId = 4,
                Title = "UML2 et les Design Patterns",
                Author = "Craig Larman",
                ISBN = "9782744070907",
                Publisher = "InfoSphère",
                NumPages = 783,
                Year = 2017,
                ReferencePrice = 60
            };
            books.Add(book);

            imgCpy = new ImageCopy(); imgCpy.Id = 5; imgCpy.ImageToBlob(Path.Combine(path, "5.jpg"), ".jpg"); images.Add(imgCpy);
            book = new Book
            {
                Id = 5,
                ImageCopyId = 5,
                Title = "Guide complet du langage C",
                Author = "Claude Delannoy",
                ISBN = "9782212140125",
                Publisher = "Polaris",
                NumPages = 389,
                Year = 2003,
                ReferencePrice = 45
            };
            books.Add(book);

            imgCpy = new ImageCopy(); imgCpy.Id = 6; imgCpy.ImageToBlob(Path.Combine(path, "6.jpg"), ".jpg"); images.Add(imgCpy);
            book = new Book
            {
                Id = 6,
                ImageCopyId = 6,
                Title = "Programmer en Java (9e édition)",
                Author = "Claude Delannoy",
                ISBN = "9782212140071",
                Publisher = "Parchemins",
                NumPages = 239,
                Year = 2014,
                ReferencePrice = 55.99
            };
            books.Add(book);

            imgCpy = new ImageCopy(); imgCpy.Id = 7; imgCpy.ImageToBlob(Path.Combine(path, "7.jpg"), ".jpg"); images.Add(imgCpy);
            book = new Book
            {
                Id = 7,
                ImageCopyId = 7,
                Title = "PHP 7 : cours et exercices",
                Author = "Jean Engels",
                ISBN = "9782212673609",
                Publisher = "InfoSphère",
                NumPages = 367,
                Year = 2009,
                ReferencePrice = 39.00
            };
            books.Add(book);

            imgCpy = new ImageCopy(); imgCpy.Id = 8; imgCpy.ImageToBlob(Path.Combine(path, "8.jpg"), ".jpg"); images.Add(imgCpy);
            book = new Book
            {
                Id = 8,
                ImageCopyId = 8,
                Title = "Algorithmique : techniques fondamentales de programmation",
                Author = "Franck Ebel",
                ISBN = "9782409012266",
                Publisher = "Parchemins",
                NumPages = 548,
                Year = 2019,
                ReferencePrice = 70
            };
            books.Add(book);

            imgCpy = new ImageCopy(); imgCpy.Id = 9; imgCpy.ImageToBlob(Path.Combine(path, "9.jpg"), ".jpg"); images.Add(imgCpy);
            book = new Book
            {
                Id = 9,
                ImageCopyId = 9,
                Title = "Les design patterns en Ruby",
                Author = "Russ Olsen",
                ISBN = "9782744022692",
                Publisher = "Parchemins",
                NumPages = 890,
                Year = 2006,
                ReferencePrice = 56.50
            };
            books.Add(book);

            imgCpy = new ImageCopy(); imgCpy.Id = 10; imgCpy.ImageToBlob(Path.Combine(path, "10.jpg"), ".jpg"); images.Add(imgCpy);
            book = new Book
            {
                Id = 10,
                ImageCopyId = 10,
                Title = "Apprendre la programmation orientée objet avec le langage C#",
                Author = "Jean Beau",
                ISBN = "9787680222692",
                Publisher = "Parchemins",
                NumPages = 690,
                Year = 2013,
                ReferencePrice = 55.90
            };
            books.Add(book);

            imgCpy = new ImageCopy(); imgCpy.Id = 11; imgCpy.ImageToBlob(Path.Combine(path, "11.jpg"), ".jpg"); images.Add(imgCpy);
            book = new Book
            {
                Id = 11,
                ImageCopyId = 11,
                Title = "Informatique et société connectées",
                Author = "Dominique Carré",
                ISBN = "9787900982612",
                Publisher = "InfoSphère",
                NumPages = 214,
                Year = 2018,
                ReferencePrice = 29.99
            };
            books.Add(book);

            imgCpy = new ImageCopy(); imgCpy.Id = 12; imgCpy.ImageToBlob(Path.Combine(path, "12.jpg"), ".jpg"); images.Add(imgCpy);
            book = new Book
            {
                Id = 12,
                ImageCopyId = 12,
                Title = "Mettre en oeuvre DevOps - 3e édition",
                Author = "Alain Sacquet",
                ISBN = "9782100819942",
                Publisher = "Parchemins",
                NumPages = 378,
                Year = 2017,
                ReferencePrice = 70
            };
            books.Add(book);

            imgCpy = new ImageCopy(); imgCpy.Id = 13; imgCpy.ImageToBlob(Path.Combine(path, "13.jpg"), ".jpg"); images.Add(imgCpy);
            book = new Book
            {
                Id = 13,
                ImageCopyId = 13,
                Title = "Linux : l'essentiel du code et des commandes",
                Author = "Scott Granneman",
                ISBN = "9782744067297",
                Publisher = "Parchemins",
                NumPages = 1290,
                Year = 2018,
                ReferencePrice = 99.00
            };
            books.Add(book);

            imgCpy = new ImageCopy(); imgCpy.Id = 14; imgCpy.ImageToBlob(Path.Combine(path, "14.jpg"), ".jpg"); images.Add(imgCpy);
            book = new Book
            {
                Id = 14,
                ImageCopyId = 14,
                Title = "Linux - Maîtrisez les commandes de base du système",
                Author = "Nicolas Pons",
                ISBN = "9782744067297",
                Publisher = "Parchemins",
                NumPages = 670,
                Year = 2009,
                ReferencePrice = 67
            };
            books.Add(book);

            imgCpy = new ImageCopy(); imgCpy.Id = 15; imgCpy.ImageToBlob(Path.Combine(path, "15.jpg"), ".jpg"); images.Add(imgCpy);
            book = new Book
            {
                Id = 15,
                ImageCopyId = 15,
                Title = "Machine Learning : les fondamentaux",
                Author = "Matt Harrison",
                ISBN = "9782412056028",
                Publisher = "InfoSphère",
                NumPages = 120,
                Year = 2020,
                ReferencePrice = 25.99
            };
            books.Add(book);

            imgCpy = new ImageCopy(); imgCpy.Id = 16; imgCpy.ImageToBlob(Path.Combine(path, "16.jpg"), ".jpg"); images.Add(imgCpy);
            book = new Book
            {
                Id = 16,
                ImageCopyId = 16,
                Title = "L'intelligence artificielle pour les développeurs",
                Author = "Virginie Mathivet",
                ISBN = "9782409017094",
                Publisher = "Parchemins",
                NumPages = 320,
                Year = 2018,
                ReferencePrice = 45.00
            };
            books.Add(book);

            imgCpy = new ImageCopy(); imgCpy.Id = 17; imgCpy.ImageToBlob(Path.Combine(path, "17.png"), ".png"); images.Add(imgCpy);
            book = new Book
            {
                Id = 17,
                ImageCopyId = 17,
                Title = "Graphes et algorithmes, 4e ed.",
                Author = "Michel Minoux, Michel Gondran",
                ISBN = "9782743018658",
                Publisher = "Parchemins",
                NumPages = 290,
                Year = 2016,
                ReferencePrice = 43.99
            };
            books.Add(book);

            imgCpy = new ImageCopy(); imgCpy.Id = 18; imgCpy.ImageToBlob(Path.Combine(path, "18.jpg"), ".jpg"); images.Add(imgCpy);
            book = new Book
            {
                Id = 18,
                ImageCopyId = 18,
                Title = "Fondamentaux de la théorie des automates",
                Author = "Patrice Séébold",
                ISBN = "9782340054110",
                Publisher = "InfoSphère",
                NumPages = 435,
                Year = 2010,
                ReferencePrice = 65.00
            };
            books.Add(book);

            imgCpy = new ImageCopy(); imgCpy.Id = 19; imgCpy.ImageToBlob(Path.Combine(path, "19.jpg"), ".jpg"); images.Add(imgCpy);
            book = new Book
            {
                Id = 19,
                ImageCopyId = 19,
                Title = "Mathématiques pour l'informatique - Exercices et problèmes",
                Author = "Jacques Vélu",
                ISBN = "9782100520527",
                Publisher = "Parchemins",
                NumPages = 674,
                Year = 2002,
                ReferencePrice = 89.00
            };
            books.Add(book);

            imgCpy = new ImageCopy(); imgCpy.Id = 20; imgCpy.ImageToBlob(Path.Combine(path, "20.jpg"), ".jpg"); images.Add(imgCpy);
            book = new Book
            {
                Id = 20,
                ImageCopyId = 20,
                Title = "Introduction to Algorithms",
                Author = "Thomas H. Cormen",
                ISBN = "9780262033848",
                Publisher = "MIT Press",
                NumPages = 1292,
                Year = 2009,
                ReferencePrice = 124.38
            };
            books.Add(book);

            imgCpy = new ImageCopy(); imgCpy.Id = 21; imgCpy.ImageToBlob(Path.Combine(path, "21.jpg"), ".jpg"); images.Add(imgCpy);
            book = new Book
            {
                Id = 21,
                ImageCopyId = 21,
                Title = "Code: The Hidden Language of Computer Hardware and Software",
                Author = "Charles Petzold",
                ISBN = "9780735638723",
                Publisher = "Microsoft Press",
                NumPages = 400,
                Year = 2000,
                ReferencePrice = 23.59
            };
            books.Add(book);

            imgCpy = new ImageCopy(); imgCpy.Id = 22; imgCpy.ImageToBlob(Path.Combine(path, "22.jpg"), ".jpg"); images.Add(imgCpy);
            book = new Book
            {
                Id = 22,
                ImageCopyId = 22,
                Title = "Design Patterns",
                Author = "Erich Gamma",
                ISBN = "9780201309515",
                Publisher = "Addison-Wesley",
                NumPages = 395,
                Year = 1995,
                ReferencePrice = 47.29
            };
            books.Add(book);

            imgCpy = new ImageCopy(); imgCpy.Id = 23; imgCpy.ImageToBlob(Path.Combine(path, "23.jpg"), ".jpg"); images.Add(imgCpy);
            book = new Book
            {
                Id = 23,
                ImageCopyId = 23,
                Title = "The Pragmatic Programmer",
                Author = "Andrew Hunt",
                ISBN = "9780132119177",
                Publisher = "MIT Press",
                NumPages = 352,
                Year = 1999,
                ReferencePrice = 39.99
            };
            books.Add(book);

            imgCpy = new ImageCopy(); imgCpy.Id = 24; imgCpy.ImageToBlob(Path.Combine(path, "24.jpg"), ".jpg"); images.Add(imgCpy);
            book = new Book
            {
                Id = 24,
                ImageCopyId = 24,
                Title = "Code Complete",
                Author = "Steve McConnell",
                ISBN = "9780735619678",
                Publisher = "Microsoft Press",
                NumPages = 960,
                Year = 2004,
                ReferencePrice = 50.00
            };
            books.Add(book);

            imgCpy = new ImageCopy(); imgCpy.Id = 25; imgCpy.ImageToBlob(Path.Combine(path, "25.jpg"), ".jpg"); images.Add(imgCpy);
            book = new Book
            {
                Id = 25,
                ImageCopyId = 25,
                Title = "Programming Pearls, 2nd Edition",
                Author = "Jon Louis Bentley",
                ISBN = "9780201657883",
                Publisher = "Addison-Wesley",
                NumPages = 239,
                Year = 1999,
                ReferencePrice = 35.49
            };
            books.Add(book);

            imgCpy = new ImageCopy(); imgCpy.Id = 26; imgCpy.ImageToBlob(Path.Combine(path, "26.jpg"), ".jpg"); images.Add(imgCpy);
            book = new Book
            {
                Id = 26,
                ImageCopyId = 26,
                Title = "Deep Learning",
                Author = "Ian Goodfellow",
                ISBN = "9780262035613",
                Publisher = "MIT Press",
                NumPages = 775,
                Year = 2016,
                ReferencePrice = 83.99
            };
            books.Add(book);

            imgCpy = new ImageCopy(); imgCpy.Id = 27; imgCpy.ImageToBlob(Path.Combine(path, "27.jpg"), ".jpg"); images.Add(imgCpy);
            book = new Book
            {
                Id = 27,
                ImageCopyId = 27,
                Title = "Prolog Programming for Artificial Intelligence",
                Author = "Ivan Bratko",
                ISBN = "9780321417466",
                Publisher = "MIT Press",
                NumPages = 890,
                Year = 1997,
                ReferencePrice = 92.45
            };
            books.Add(book);

            imgCpy = new ImageCopy(); imgCpy.Id = 28; imgCpy.ImageToBlob(Path.Combine(path, "28.png"), ".png"); images.Add(imgCpy);
            book = new Book
            {
                Id = 28,
                ImageCopyId = 28,
                Title = "Haskell Programming",
                Author = "Christopher Allen",
                ISBN = "9180621418476",
                Publisher = "MIT Press",
                NumPages = 130,
                Year = 2021,
                ReferencePrice = 49.00
            };
            books.Add(book);

            imgCpy = new ImageCopy(); imgCpy.Id = 29; imgCpy.ImageToBlob(Path.Combine(path, "29.jpg"), ".jpg"); images.Add(imgCpy);
            book = new Book
            {
                Id = 29,
                ImageCopyId = 29,
                Title = "Haskell from the Very Beginning",
                Author = "John Whitington",
                ISBN = "9780957671133",
                Publisher = "MIT Press",
                NumPages = 490,
                Year = 2019,
                ReferencePrice = 45.00
            };
            books.Add(book);

            imgCpy = new ImageCopy(); imgCpy.Id = 30; imgCpy.ImageToBlob(Path.Combine(path, "30.jpg"), ".jpg"); images.Add(imgCpy);
            book = new Book
            {
                Id = 30,
                ImageCopyId = 30,
                Title = "Mathématiques pour l'informatique",
                Author = "Christian Contant",
                ISBN = "9782894618622",
                Publisher = "Chenelière",
                NumPages = 424,
                Year = 2002,
                ReferencePrice = 79.95
            };
            books.Add(book);

            imgCpy = new ImageCopy(); imgCpy.Id = 31; imgCpy.ImageToBlob(Path.Combine(path, "31.jpg"), ".jpg"); images.Add(imgCpy);
            book = new Book
            {
                Id = 31,
                ImageCopyId = 31,
                Title = "Uml 2.5 par la pratique",
                Author = "Pascal Roques",
                ISBN = "9782826632622",
                Publisher = "Chenelière",
                NumPages = 408,
                Year = 2018,
                ReferencePrice = 63.99
            };
            books.Add(book);

            imgCpy = new ImageCopy(); imgCpy.Id = 32; imgCpy.ImageToBlob(Path.Combine(path, "32.jpg"), ".jpg"); images.Add(imgCpy);
            book = new Book
            {
                Id = 32,
                ImageCopyId = 32,
                Title = "Intégration web: les bonnes pratiques",
                Author = "Corinne Schillinger",
                ISBN = "9782212133707",
                Publisher = "Eyrolles",
                NumPages = 780,
                Year = 2012,
                ReferencePrice = 57.95
            };
            books.Add(book);

            imgCpy = new ImageCopy(); imgCpy.Id = 33; imgCpy.ImageToBlob(Path.Combine(path, "33.jpg"), ".jpg"); images.Add(imgCpy);
            book = new Book
            {
                Id = 33,
                ImageCopyId = 33,
                Title = "Artificial Intelligence: A Modern Approach",
                Author = "Stuart Russell",
                ISBN = "9362882109707",
                Publisher = "MIT Press",
                NumPages = 589,
                Year = 2020,
                ReferencePrice = 189.99
            };
            books.Add(book);

            imgCpy = new ImageCopy(); imgCpy.Id = 34; imgCpy.ImageToBlob(Path.Combine(path, "34.jpg"), ".jpg"); images.Add(imgCpy);
            book = new Book
            {
                Id = 34,
                ImageCopyId = 34,
                Title = "Computer Networking: A Top-Down Approach",
                Author = "James Kurose",
                ISBN = "9389812163707",
                Publisher = "Corner Editions",
                NumPages = 890,
                Year = 2016,
                ReferencePrice = 198.38
            };
            books.Add(book);

            imgCpy = new ImageCopy(); imgCpy.Id = 35; imgCpy.ImageToBlob(Path.Combine(path, "35.jpg"), ".jpg"); images.Add(imgCpy);
            book = new Book
            {
                Id = 35,
                ImageCopyId = 35,
                Title = "Responsive Web Design with HTML5 and CSS3",
                Author = "Ben Frain",
                ISBN = "9983872167807",
                Publisher = "Packt",
                NumPages = 490,
                Year = 2012,
                ReferencePrice = 27.48
            };
            books.Add(book);

            imgCpy = new ImageCopy(); imgCpy.Id = 36; imgCpy.ImageToBlob(Path.Combine(path, "36.jpg"), ".jpg"); images.Add(imgCpy);
            book = new Book
            {
                Id = 36,
                ImageCopyId = 36,
                Title = "Interactive Front-End Web Development",
                Author = "Jon Duckett",
                ISBN = "9984472907856",
                Publisher = "Times",
                NumPages = 321,
                Year = 2014,
                ReferencePrice = 38.49
            };
            books.Add(book);

            imgCpy = new ImageCopy(); imgCpy.Id = 37; imgCpy.ImageToBlob(Path.Combine(path, "37.jpg"), ".jpg"); images.Add(imgCpy);
            book = new Book
            {
                Id = 37,
                ImageCopyId = 37,
                Title = "Guide to Operating Systems",
                Author = "Greg Tomsho",
                ISBN = "9978468907878",
                Publisher = "MIT Press",
                NumPages = 721,
                Year = 2016,
                ReferencePrice = 133.63
            };
            books.Add(book);

            imgCpy = new ImageCopy(); imgCpy.Id = 38; imgCpy.ImageToBlob(Path.Combine(path, "38.jpg"), ".jpg"); images.Add(imgCpy);
            book = new Book
            {
                Id = 38,
                ImageCopyId = 38,
                Title = "Operating System Concepts",
                Author = "Abraham Silberschatz",
                ISBN = "9978988901324",
                Publisher = "Polaris",
                NumPages = 429,
                Year = 2019,
                ReferencePrice = 84.00
            };
            books.Add(book);

            imgCpy = new ImageCopy(); imgCpy.Id = 39; imgCpy.ImageToBlob(Path.Combine(path, "39.jpg"), ".jpg"); images.Add(imgCpy);
            book = new Book
            {
                Id = 39,
                ImageCopyId = 39,
                Title = "The Art of Assembly Language",
                Author = "Randall Hyde",
                ISBN = "9781593272074",
                Publisher = "No starch press",
                NumPages = 760,
                Year = 2010,
                ReferencePrice = 59.95
            };
            books.Add(book);

            imgCpy = new ImageCopy(); imgCpy.Id = 40; imgCpy.ImageToBlob(Path.Combine(path, "40.jpg"), ".jpg"); images.Add(imgCpy);
            book = new Book
            {
                Id = 40,
                ImageCopyId = 40,
                Title = "Modern X86 Assembly Language Programming",
                Author = "Daniel Kusswurm",
                ISBN = "9781484200643",
                Publisher = "Apress",
                NumPages = 595,
                Year = 2014,
                ReferencePrice = 64.99
            };
            books.Add(book);

            imgCpy = new ImageCopy(); imgCpy.Id = 41; imgCpy.ImageToBlob(Path.Combine(path, "41.png"), ".png"); images.Add(imgCpy);
            book = new Book
            {
                Id = 41,
                ImageCopyId = 41,
                Title = "Programming from the Ground Up",
                Author = "Jonathan Bartlett",
                ISBN = "9781616100643",
                Publisher = "MIT Press",
                NumPages = 390,
                Year = 2004,
                ReferencePrice = 37.00
            };
            books.Add(book);

            imgCpy = new ImageCopy(); imgCpy.Id = 42; imgCpy.ImageToBlob(Path.Combine(path, "42.jpg"), ".jpg"); images.Add(imgCpy);
            book = new Book
            {
                Id = 42,
                ImageCopyId = 42,
                Title = "Node.js: exploitez la puissance de JavaScript côté serveur",
                Author = "Julien Fontanet",
                ISBN = "9782746089785",
                Publisher = "ENI",
                NumPages = 612,
                Year = 2015,
                ReferencePrice = 59.95
            };
            books.Add(book);

            imgCpy = new ImageCopy(); imgCpy.Id = 43; imgCpy.ImageToBlob(Path.Combine(path, "43.jpg"), ".jpg"); images.Add(imgCpy);
            book = new Book
            {
                Id = 43,
                ImageCopyId = 43,
                Title = "Projects in Computing and Information Systems: A Student's Guide",
                Author = "Christian Dawson",
                ISBN = "9780273721314",
                Publisher = "Polaris",
                NumPages = 304,
                Year = 2009,
                ReferencePrice = 19.96
            };
            books.Add(book);

            imgCpy = new ImageCopy(); imgCpy.Id = 44; imgCpy.ImageToBlob(Path.Combine(path, "44.jpg"), ".jpg"); images.Add(imgCpy);
            book = new Book
            {
                Id = 44,
                ImageCopyId = 44,
                Title = "Writing for Computer Science",
                Author = "Justin Zobel",
                ISBN = "9781852338022",
                Publisher = "Polaris",
                NumPages = 270,
                Year = 2004,
                ReferencePrice = 104.15
            };
            books.Add(book);

            imgCpy = new ImageCopy(); imgCpy.Id = 45; imgCpy.ImageToBlob(Path.Combine(path, "45.jpg"), ".jpg"); images.Add(imgCpy);
            book = new Book
            {
                Id = 45,
                ImageCopyId = 45,
                Title = "Fundamentals of Information Systems Security",
                Author = "David Kim",
                ISBN = "9781284116458",
                Publisher = "Polaris",
                NumPages = 548,
                Year = 2016,
                ReferencePrice = 129.04
            };
            books.Add(book);

            imgCpy = new ImageCopy(); imgCpy.Id = 46; imgCpy.ImageToBlob(Path.Combine(path, "46.jpg"), ".jpg"); images.Add(imgCpy);
            book = new Book
            {
                Id = 46,
                ImageCopyId = 46,
                Title = "Network and System Security",
                Author = "John R. Vacca",
                ISBN = "9780124166899",
                Publisher = "Polaris",
                NumPages = 432,
                Year = 2014,
                ReferencePrice = 41.96
            };
            books.Add(book);

            imgCpy = new ImageCopy(); imgCpy.Id = 47; imgCpy.ImageToBlob(Path.Combine(path, "47.jpg"), ".jpg"); images.Add(imgCpy);
            book = new Book
            {
                Id = 47,
                ImageCopyId = 47,
                Title = "Handbook of Simulation",
                Author = "Jerry Banks",
                ISBN = "9780471134039",
                Publisher = "Polaris",
                NumPages = 864,
                Year = 1998,
                ReferencePrice = 108.95
            };
            books.Add(book);

            imgCpy = new ImageCopy(); imgCpy.Id = 48; imgCpy.ImageToBlob(Path.Combine(path, "48.jpg"), ".jpg"); images.Add(imgCpy);
            book = new Book
            {
                Id = 48,
                ImageCopyId = 48,
                Title = "Computational Linguistics and Talking Robots",
                Author = "Roland Hausser",
                ISBN = "9783642224317",
                Publisher = "Polaris",
                NumPages = 286,
                Year = 2011,
                ReferencePrice = 189.00
            };
            books.Add(book);

            imgCpy = new ImageCopy(); imgCpy.Id = 49; imgCpy.ImageToBlob(Path.Combine(path, "49.jpg"), ".jpg"); images.Add(imgCpy);
            book = new Book
            {
                Id = 49,
                ImageCopyId = 49,
                Title = "Automaton Theories of Human Sentence Comprehension",
                Author = "John T. Hale",
                ISBN = "9781575867472",
                Publisher = "Center for the Study of Language",
                NumPages = 204,
                Year = 2014,
                ReferencePrice = 37.20
            };
            books.Add(book);

            imgCpy = new ImageCopy(); imgCpy.Id = 50; imgCpy.ImageToBlob(Path.Combine(path, "50.jpg"), ".jpg"); images.Add(imgCpy);
            book = new Book
            {
                Id = 50,
                ImageCopyId = 50,
                Title = "Algorithms to Live By: The Computer Science of Human Decisions",
                Author = "Brian Christian",
                ISBN = "9781627790369",
                Publisher = "Henry Holt and Co",
                NumPages = 368,
                Year = 2016,
                ReferencePrice = 32.18
            };
            books.Add(book);

            imgCpy = new ImageCopy(); imgCpy.Id = 51; imgCpy.ImageToBlob(Path.Combine(path, "51.jpg"), ".jpg"); images.Add(imgCpy);
            book = new Book
            {
                Id = 51,
                ImageCopyId = 51,
                Title = "Algorithm Design",
                Author = "Jon Kleinberg",
                ISBN = "9780321295354",
                Publisher = "Polaris",
                NumPages = 864,
                Year = 2005,
                ReferencePrice = 193.80
            };
            books.Add(book);

            imgCpy = new ImageCopy(); imgCpy.Id = 52; imgCpy.ImageToBlob(Path.Combine(path, "52.jpg"), ".jpg"); images.Add(imgCpy);
            book = new Book
            {
                Id = 52,
                ImageCopyId = 52,
                Title = "Computer Networks (5th Edition)",
                Author = "Andrew S. Tanenbaum",
                ISBN = "9780132126953",
                Publisher = "Polaris",
                NumPages = 960,
                Year = 2010,
                ReferencePrice = 232.95
            };
            books.Add(book);

            imgCpy = new ImageCopy(); imgCpy.Id = 53; imgCpy.ImageToBlob(Path.Combine(path, "53.jpg"), ".jpg"); images.Add(imgCpy);
            book = new Book
            {
                Id = 53,
                ImageCopyId = 53,
                Title = "Modern Operating Systems: Global Edition",
                Author = "Herbert Bos",
                ISBN = "9781292061429",
                Publisher = "Polaris",
                NumPages = 1024,
                Year = 2016,
                ReferencePrice = 88.00
            };
            books.Add(book);

            imgCpy = new ImageCopy(); imgCpy.Id = 54; imgCpy.ImageToBlob(Path.Combine(path, "54.jpg"), ".jpg"); images.Add(imgCpy);
            book = new Book
            {
                Id = 54,
                ImageCopyId = 54,
                Title = "Data Structures and Algorithm Analysis in C++",
                Author = "Mark A. Weiss",
                ISBN = "9780273769385",
                Publisher = "Polaris",
                NumPages = 659,
                Year = 2012,
                ReferencePrice = 88.00
            };
            books.Add(book);

            imgCpy = new ImageCopy(); imgCpy.Id = 55; imgCpy.ImageToBlob(Path.Combine(path, "55.jpg"), ".jpg"); images.Add(imgCpy);
            book = new Book
            {
                Id = 55,
                ImageCopyId = 55,
                Title = "Coder proprement",
                Author = "Robert C. Martin",
                ISBN = "9782326002272",
                Publisher = "Polaris",
                NumPages = 448,
                Year = 2019,
                ReferencePrice = 67.95
            };
            books.Add(book);

            imgCpy = new ImageCopy(); imgCpy.Id = 56; imgCpy.ImageToBlob(Path.Combine(path, "56.jpg"), ".jpg"); images.Add(imgCpy);
            book = new Book
            {
                Id = 56,
                ImageCopyId = 56,
                Title = "Langage c : maîtriser la programmation procédurale",
                Author = "Frédéric Drouillon",
                ISBN = "9782409014000",
                Publisher = "ENI",
                NumPages = 615,
                Year = 2018,
                ReferencePrice = 49.95
            };
            books.Add(book);

            imgCpy = new ImageCopy(); imgCpy.Id = 57; imgCpy.ImageToBlob(Path.Combine(path, "57.jpg"), ".jpg"); images.Add(imgCpy);
            book = new Book
            {
                Id = 57,
                ImageCopyId = 57,
                Title = "Gestion des tests logiciels : Bonnes pratiques à mettre en oeuvre",
                Author = "Emmanuel Itié",
                ISBN = "9782409000300",
                Publisher = "ENI",
                NumPages = 388,
                Year = 2016,
                ReferencePrice = 69.95
            };
            books.Add(book);

            imgCpy = new ImageCopy(); imgCpy.Id = 58; imgCpy.ImageToBlob(Path.Combine(path, "58.jpg"), ".jpg"); images.Add(imgCpy);
            book = new Book
            {
                Id = 58,
                ImageCopyId = 58,
                Title = "Conception et programmation orientées objet",
                Author = "Bertrand Meyer",
                ISBN = "9782212675009",
                Publisher = "Eyrolles",
                NumPages = 703,
                Year = 2017,
                ReferencePrice = 114.95
            };
            books.Add(book);

            imgCpy = new ImageCopy(); imgCpy.Id = 59; imgCpy.ImageToBlob(Path.Combine(path, "59.jpg"), ".jpg"); images.Add(imgCpy);
            book = new Book
            {
                Id = 59,
                ImageCopyId = 59,
                Title = "Programmation Shell sous Unix",
                Author = "Christine Deffaix Rémy",
                ISBN = "9782746067141",
                Publisher = "ENI",
                NumPages = 280,
                Year = 2014,
                ReferencePrice = 44.95
            };
            books.Add(book);

            imgCpy = new ImageCopy(); imgCpy.Id = 60; imgCpy.ImageToBlob(Path.Combine(path, "60.jpg"), ".jpg"); images.Add(imgCpy);
            book = new Book
            {
                Id = 60,
                ImageCopyId = 60,
                Title = "Design patterns en PHP: les 23 modèles de conception",
                Author = "Laurent Debrauwer",
                ISBN = "9782746093218",
                Publisher = "ENI",
                NumPages = 450,
                Year = 2017,
                ReferencePrice = 59.95
            };
            books.Add(book);

            imgCpy = new ImageCopy(); imgCpy.Id = 61; imgCpy.ImageToBlob(Path.Combine(path, "61.jpg"), ".jpg"); images.Add(imgCpy);
            book = new Book
            {
                Id = 61,
                ImageCopyId = 61,
                Title = "Découvrez le langage Javascript",
                Author = "Johann Pardanaud",
                ISBN = "9782212143997",
                Publisher = "Eyrolles",
                NumPages = 561,
                Year = 2018,
                ReferencePrice = 54.95
            };
            books.Add(book);

            imgCpy = new ImageCopy(); imgCpy.Id = 62; imgCpy.ImageToBlob(Path.Combine(path, "62.jpg"), ".jpg"); images.Add(imgCpy);
            book = new Book
            {
                Id = 62,
                ImageCopyId = 62,
                Title = "Analyse des besoins pour le développement logiciel",
                Author = "Jacques Lonchamp",
                ISBN = "9782100727148",
                Publisher = "Info Sup",
                NumPages = 377,
                Year = 2015,
                ReferencePrice = 46.95
            };
            books.Add(book);

            imgCpy = new ImageCopy(); imgCpy.Id = 63; imgCpy.ImageToBlob(Path.Combine(path, "63.jpg"), ".jpg"); images.Add(imgCpy);
            book = new Book
            {
                Id = 63,
                ImageCopyId = 63,
                Title = "Cartographie des réseaux: L'art de représenter la complexité",
                Author = "Manuel Lima",
                ISBN = "9782212135695",
                Publisher = "Eyrolles",
                NumPages = 480,
                Year = 2013,
                ReferencePrice = 72.95
            };
            books.Add(book);

            imgCpy = new ImageCopy(); imgCpy.Id = 64; imgCpy.ImageToBlob(Path.Combine(path, "64.jpg"), ".jpg"); images.Add(imgCpy);
            book = new Book
            {
                Id = 64,
                ImageCopyId = 64,
                Title = "Git par la pratique",
                Author = "David Demaree",
                ISBN = "9782212674415",
                Publisher = "Eyrolles",
                NumPages = 210,
                Year = 2017,
                ReferencePrice = 27.95
            };
            books.Add(book);

            imgCpy = new ImageCopy(); imgCpy.Id = 65; imgCpy.ImageToBlob(Path.Combine(path, "65.jpg"), ".jpg"); images.Add(imgCpy);
            book = new Book
            {
                Id = 65,
                ImageCopyId = 65,
                Title = "Réseaux informatiques - Notions fondamentales",
                Author = "José Dordoigne",
                ISBN = "9782409021398",
                Publisher = "ENI",
                NumPages = 766,
                Year = 2020,
                ReferencePrice = 49.95
            };
            books.Add(book);

            imgCpy = new ImageCopy(); imgCpy.Id = 66; imgCpy.ImageToBlob(Path.Combine(path, "66.jpg"), ".jpg"); images.Add(imgCpy);
            book = new Book
            {
                Id = 66,
                ImageCopyId = 66,
                Title = "Intelligence artificielle vulgarisée",
                Author = "Aurélien Vannieuwenhuyze",
                ISBN = "9782409020735",
                Publisher = "ENI",
                NumPages = 434,
                Year = 2020,
                ReferencePrice = 49.95
            };
            books.Add(book);

            imgCpy = new ImageCopy(); imgCpy.Id = 67; imgCpy.ImageToBlob(Path.Combine(path, "67.jpg"), ".jpg"); images.Add(imgCpy);
            book = new Book
            {
                Id = 67,
                ImageCopyId = 67,
                Title = "Gestion d'un projet web",
                Author = "Vincent Hiard",
                ISBN = "9782409019128",
                Publisher = "ENI",
                NumPages = 315,
                Year = 2019,
                ReferencePrice = 74.95
            };
            books.Add(book);

            imgCpy = new ImageCopy(); imgCpy.Id = 68; imgCpy.ImageToBlob(Path.Combine(path, "68.jpg"), ".jpg"); images.Add(imgCpy);
            book = new Book
            {
                Id = 68,
                ImageCopyId = 68,
                Title = "Git : Maîtrisez la gestion de vos versions",
                Author = "Samuel Dauzon",
                ISBN = "9782409019104",
                Publisher = "ENI",
                NumPages = 377,
                Year = 2019,
                ReferencePrice = 89.95
            };
            books.Add(book);

            imgCpy = new ImageCopy(); imgCpy.Id = 69; imgCpy.ImageToBlob(Path.Combine(path, "69.jpg"), ".jpg"); images.Add(imgCpy);
            book = new Book
            {
                Id = 69,
                ImageCopyId = 69,
                Title = "Conduite de projets informatiques",
                Author = "Brice-Arnaud Guérin",
                ISBN = "9782409014635",
                Publisher = "ENI",
                NumPages = 447,
                Year = 2018,
                ReferencePrice = 69.95
            };
            books.Add(book);

            imgCpy = new ImageCopy(); imgCpy.Id = 70; imgCpy.ImageToBlob(Path.Combine(path, "70.jpg"), ".jpg"); images.Add(imgCpy);
            book = new Book
            {
                Id = 70,
                ImageCopyId = 70,
                Title = "Architecture et technologie des ordinateurs",
                Author = "Paoló Zanella",
                ISBN = "9782100784592",
                Publisher = "Eyrolles",
                NumPages = 592,
                Year = 2018,
                ReferencePrice = 45.99
            };
            books.Add(book);

            imgCpy = new ImageCopy(); imgCpy.Id = 71; imgCpy.ImageToBlob(Path.Combine(path, "71.jpg"), ".jpg"); images.Add(imgCpy);
            book = new Book
            {
                Id = 71,
                ImageCopyId = 71,
                Title = "UML2 pour les développeurs",
                Author = "Xavier Blanc",
                ISBN = "9782212120295",
                Publisher = "Eyrolles",
                NumPages = 202,
                Year = 2006,
                ReferencePrice = 25.95
            };
            books.Add(book);

            imgCpy = new ImageCopy(); imgCpy.Id = 72; imgCpy.ImageToBlob(Path.Combine(path, "72.jpg"), ".jpg"); images.Add(imgCpy);
            book = new Book
            {
                Id = 72,
                ImageCopyId = 72,
                Title = "Logique pour l'informatique",
                Author = "Mathieu Jaume",
                ISBN = "9782340042612",
                Publisher = "Eyrolles",
                NumPages = 342,
                Year = 2020,
                ReferencePrice = 43.00
            };
            books.Add(book);

            imgCpy = new ImageCopy(); imgCpy.Id = 73; imgCpy.ImageToBlob(Path.Combine(path, "73.jpg"), ".jpg"); images.Add(imgCpy);
            book = new Book
            {
                Id = 73,
                ImageCopyId = 73,
                Title = "Mini-manuel de programmation fonctionnelle",
                Author = "Eric Violard",
                ISBN = "9782100703852",
                Publisher = "Eyrolles",
                NumPages = 220,
                Year = 2014,
                ReferencePrice = 24.95
            };
            books.Add(book);

            imgCpy = new ImageCopy(); imgCpy.Id = 74; imgCpy.ImageToBlob(Path.Combine(path, "74.jpg"), ".jpg"); images.Add(imgCpy);
            book = new Book
            {
                Id = 74,
                ImageCopyId = 74,
                Title = "Introduction au deep learning",
                Author = "Eugene Charniak",
                ISBN = "9782100819263",
                Publisher = "Eyrolles",
                NumPages = 16,
                Year = 2020,
                ReferencePrice = 32.95
            };
            books.Add(book);

            imgCpy = new ImageCopy(); imgCpy.Id = 75; imgCpy.ImageToBlob(Path.Combine(path, "75.jpg"), ".jpg"); images.Add(imgCpy);
            book = new Book
            {
                Id = 75,
                ImageCopyId = 75,
                Title = "Théorie des graphes et applications",
                Author = "Jean-Claude Fournier",
                ISBN = "9782746232150",
                Publisher = "Eyrolles",
                NumPages = 332,
                Year = 2011,
                ReferencePrice = 119.95
            };
            books.Add(book);

            imgCpy = new ImageCopy(); imgCpy.Id = 76; imgCpy.ImageToBlob(Path.Combine(path, "76.jpg"), ".jpg"); images.Add(imgCpy);
            book = new Book
            {
                Id = 76,
                ImageCopyId = 76,
                Title = "Apprentissage statistique",
                Author = "Gérard Dreyfus",
                ISBN = "9782212122299",
                Publisher = "Eyrolles",
                NumPages = 448,
                Year = 2008,
                ReferencePrice = 84.95
            };
            books.Add(book);

            imgCpy = new ImageCopy(); imgCpy.Id = 77; imgCpy.ImageToBlob(Path.Combine(path, "77.jpg"), ".jpg"); images.Add(imgCpy);
            book = new Book
            {
                Id = 77,
                ImageCopyId = 77,
                Title = "Bases de données: Concepts, utilisation et développement",
                Author = "Jean-Luc Hainaut",
                ISBN = "9782100790685",
                Publisher = "Eyrolles",
                NumPages = 736,
                Year = 2018,
                ReferencePrice = 54.99
            };
            books.Add(book);

            imgCpy = new ImageCopy(); imgCpy.Id = 78; imgCpy.ImageToBlob(Path.Combine(path, "78.jpg"), ".jpg"); images.Add(imgCpy);
            book = new Book
            {
                Id = 78,
                ImageCopyId = 78,
                Title = "Programmation - Le langage SQL",
                Author = "Frédéric Baurand",
                ISBN = "9782729870805",
                Publisher = "Eyrolles",
                NumPages = 187,
                Year = 2011,
                ReferencePrice = 29.95
            };
            books.Add(book);

            imgCpy = new ImageCopy(); imgCpy.Id = 79; imgCpy.ImageToBlob(Path.Combine(path, "79.jpg"), ".jpg"); images.Add(imgCpy);
            book = new Book
            {
                Id = 79,
                ImageCopyId = 79,
                Title = "SQL par l'exemple",
                Author = "Sylvain Berger",
                ISBN = "9782340016620",
                Publisher = "Eyrolles",
                NumPages = 264,
                Year = 2017,
                ReferencePrice = 41.95
            };
            books.Add(book);

            imgCpy = new ImageCopy(); imgCpy.Id = 80; imgCpy.ImageToBlob(Path.Combine(path, "80.jpg"), ".jpg"); images.Add(imgCpy);
            book = new Book
            {
                Id = 80,
                ImageCopyId = 80,
                Title = "Java for programmers",
                Author = "Deitel Paul J.",
                ISBN = "9780132821544",
                Publisher = "Prentice Hall",
                NumPages = 1168,
                Year = 2009,
                ReferencePrice = 78.95
            };
            books.Add(book);

            imgCpy = new ImageCopy(); imgCpy.Id = 81; imgCpy.ImageToBlob(Path.Combine(path, "81.jpg"), ".jpg"); images.Add(imgCpy);
            book = new Book
            {
                Id = 81,
                ImageCopyId = 81,
                Title = "Fondements de la programmation: Concepts et techniques",
                Author = "Jean-Marie Rifflet",
                ISBN = "9782340000148",
                Publisher = "Ellipses",
                NumPages = 260,
                Year = 2014,
                ReferencePrice = 39.95
            };
            books.Add(book);

            imgCpy = new ImageCopy(); imgCpy.Id = 82; imgCpy.ImageToBlob(Path.Combine(path, "82.jpg"), ".jpg"); images.Add(imgCpy);
            book = new Book
            {
                Id = 82,
                ImageCopyId = 82,
                Title = "Réseaux et transmissions",
                Author = "Stéphane Lohier",
                ISBN = "9782100811830",
                Publisher = "Dunod",
                NumPages = 326,
                Year = 2020,
                ReferencePrice = 34.99
            };
            books.Add(book);

            imgCpy = new ImageCopy(); imgCpy.Id = 83; imgCpy.ImageToBlob(Path.Combine(path, "83.jpg"), ".jpg"); images.Add(imgCpy);
            book = new Book
            {
                Id = 83,
                ImageCopyId = 83,
                Title = "La programmation fonctionnelle",
                Author = "Stéphane Lohier",
                ISBN = "9782340028777",
                Publisher = "Technip",
                NumPages = 264,
                Year = 2019,
                ReferencePrice = 45.95
            };
            books.Add(book);

            imgCpy = new ImageCopy(); imgCpy.Id = 84; imgCpy.ImageToBlob(Path.Combine(path, "84.jpg"), ".jpg"); images.Add(imgCpy);
            book = new Book
            {
                Id = 84,
                ImageCopyId = 84,
                Title = "Optimisation combinatoire par métaheuristique",
                Author = "Khaled Ghedria",
                ISBN = "9782710808756",
                Publisher = "Technip",
                NumPages = 118,
                Year = 2007,
                ReferencePrice = 24.95
            };
            books.Add(book);

            imgCpy = new ImageCopy(); imgCpy.Id = 85; imgCpy.ImageToBlob(Path.Combine(path, "85.jpg"), ".jpg"); images.Add(imgCpy);
            book = new Book
            {
                Id = 85,
                ImageCopyId = 85,
                Title = "Sécurisation des architectures informatiques",
                Author = "Jean-Louis Boulanger",
                ISBN = "9782746219915",
                Publisher = "Hermès - Lavoisier",
                NumPages = 420,
                Year = 2009,
                ReferencePrice = 117.95
            };
            books.Add(book);

            imgCpy = new ImageCopy(); imgCpy.Id = 86; imgCpy.ImageToBlob(Path.Combine(path, "86.jpg"), ".jpg"); images.Add(imgCpy);
            book = new Book
            {
                Id = 86,
                ImageCopyId = 86,
                Title = "Sécurité informatique et malwares",
                Author = "Paul Rascagnères",
                ISBN = "9782409021299",
                Publisher = "ENI",
                NumPages = 474,
                Year = 2019,
                ReferencePrice = 69.95
            };
            books.Add(book);

            imgCpy = new ImageCopy(); imgCpy.Id = 87; imgCpy.ImageToBlob(Path.Combine(path, "87.jpg"), ".jpg"); images.Add(imgCpy);
            book = new Book
            {
                Id = 87,
                ImageCopyId = 87,
                Title = "Compilation : analyse lexicale et syntaxique",
                Author = "Romain Legendre",
                ISBN = "9782340003668",
                Publisher = "Ellipses",
                NumPages = 312,
                Year = 2015,
                ReferencePrice = 43.95
            };
            books.Add(book);

            imgCpy = new ImageCopy(); imgCpy.Id = 88; imgCpy.ImageToBlob(Path.Combine(path, "88.jpg"), ".jpg"); images.Add(imgCpy);
            book = new Book
            {
                Id = 88,
                ImageCopyId = 88,
                Title = "Langages informatiques: Analyse syntaxique et traduction",
                Author = "Ali Aït-El-Hadj",
                ISBN = "9782340003668",
                Publisher = "Ellipses",
                NumPages = 348,
                Year = 2015,
                ReferencePrice = 54.95
            };
            books.Add(book);

            imgCpy = new ImageCopy(); imgCpy.Id = 89; imgCpy.ImageToBlob(Path.Combine(path, "89.jpg"), ".jpg"); images.Add(imgCpy);
            book = new Book
            {
                Id = 89,
                ImageCopyId = 89,
                Title = "Théorie des langages et compilation",
                Author = "Smaïl Aït-El-Hadj",
                ISBN = "9782340027961",
                Publisher = "Ellipses",
                NumPages = 304,
                Year = 2018,
                ReferencePrice = 58.95
            };
            books.Add(book);

            imgCpy = new ImageCopy(); imgCpy.Id = 90; imgCpy.ImageToBlob(Path.Combine(path, "90.jpg"), ".jpg"); images.Add(imgCpy);
            book = new Book
            {
                Id = 90,
                ImageCopyId = 90,
                Title = "Traitement automatique des langues naturelles",
                Author = "Ismaïl Biskri",
                ISBN = "9782746231382",
                Publisher = "Hermès - Lavoisier",
                NumPages = 256,
                Year = 2011,
                ReferencePrice = 97.95
            };
            books.Add(book);

            imgCpy = new ImageCopy(); imgCpy.Id = 91; imgCpy.ImageToBlob(Path.Combine(path, "91.jpg"), ".jpg"); images.Add(imgCpy);
            book = new Book
            {
                Id = 91,
                ImageCopyId = 91,
                Title = "La formalisation des langues",
                Author = "Max Silberztein",
                ISBN = "9781784050535",
                Publisher = "Iste",
                NumPages = 425,
                Year = 2015,
                ReferencePrice = 112.95
            };
            books.Add(book);

            imgCpy = new ImageCopy(); imgCpy.Id = 92; imgCpy.ImageToBlob(Path.Combine(path, "92.jpg"), ".jpg"); images.Add(imgCpy);
            book = new Book
            {
                Id = 92,
                ImageCopyId = 92,
                Title = "Parlez-vous shell ?",
                Author = "Thomas Hugel",
                ISBN = "9782379792427",
                Publisher = "Iggybook",
                NumPages = 152,
                Year = 2020,
                ReferencePrice = 22.95
            };
            books.Add(book);

            imgCpy = new ImageCopy(); imgCpy.Id = 93; imgCpy.ImageToBlob(Path.Combine(path, "93.jpg"), ".jpg"); images.Add(imgCpy);
            book = new Book
            {
                Id = 93,
                ImageCopyId = 93,
                Title = "Le langage assembleur",
                Author = "Olivier Cauet",
                ISBN = "9782746065086",
                Publisher = "ENI",
                NumPages = 420,
                Year = 2011,
                ReferencePrice = 77.95
            };
            books.Add(book);

            #endregion

            // si mode random
            if (RANDOM)
            {
                // si on veut moins de livres que le max
                if (NUM_BOOKS < books.Count)
                {
                    // suffle des livres et images
                    int numBooks = books.Count;
                    while (numBooks > 1)
                    {
                        numBooks--;
                        int id = rnd.Next(numBooks + 1);
                        Book b = books[id];
                        ImageCopy i = images[id];
                        books[id] = books[numBooks];
                        images[id] = images[numBooks];
                        books[numBooks] = b;
                        images[numBooks] = i;
                    }

                    // on prend les NUM_BOOKS première entrées
                    books = books.Take(NUM_BOOKS).ToList();
                    images = images.Take(NUM_BOOKS).ToList();
                }
            }

            // ajout à la BD
            modelBuilder.Entity<ImageCopy>().HasData(images);
            modelBuilder.Entity<Book>().HasData(books);

            #endregion

            #region cours-livres

            List<AcademicCourseBook> courseBook = new List<AcademicCourseBook>();

            courseBook.AddCourseBook(4, 1, books);
            courseBook.AddCourseBook(2, 2, books);
            courseBook.AddCourseBook(4, 3, books);
            courseBook.AddCourseBook(8, 4, books);
            courseBook.AddCourseBook(10, 4, books);
            courseBook.AddCourseBook(5, 5, books);
            courseBook.AddCourseBook(2, 6, books);
            courseBook.AddCourseBook(15, 7, books);
            courseBook.AddCourseBook(4, 8, books);
            courseBook.AddCourseBook(11, 8, books);
            courseBook.AddCourseBook(8, 9, books);
            courseBook.AddCourseBook(2, 10, books);
            courseBook.AddCourseBook(9, 11, books);
            courseBook.AddCourseBook(10, 12, books);
            courseBook.AddCourseBook(1, 13, books);
            courseBook.AddCourseBook(1, 14, books);
            courseBook.AddCourseBook(14, 15, books);
            courseBook.AddCourseBook(14, 16, books);
            courseBook.AddCourseBook(11, 17, books);
            courseBook.AddCourseBook(12, 18, books);
            courseBook.AddCourseBook(6, 19, books);
            courseBook.AddCourseBook(4, 20, books);
            courseBook.AddCourseBook(3, 21, books);
            courseBook.AddCourseBook(16, 22, books);
            courseBook.AddCourseBook(16, 23, books);
            courseBook.AddCourseBook(3, 24, books);
            courseBook.AddCourseBook(3, 25, books);
            courseBook.AddCourseBook(17, 26, books);
            courseBook.AddCourseBook(7, 27, books);
            courseBook.AddCourseBook(14, 27, books);
            courseBook.AddCourseBook(17, 27, books);
            courseBook.AddCourseBook(7, 28, books);
            courseBook.AddCourseBook(7, 29, books);
            courseBook.AddCourseBook(6, 30, books);
            courseBook.AddCourseBook(8, 31, books);
            courseBook.AddCourseBook(10, 31, books);
            courseBook.AddCourseBook(16, 31, books);
            courseBook.AddCourseBook(15, 32, books);
            courseBook.AddCourseBook(14, 33, books);
            courseBook.AddCourseBook(17, 33, books);
            courseBook.AddCourseBook(21, 34, books);
            courseBook.AddCourseBook(22, 35, books);
            courseBook.AddCourseBook(22, 36, books);
            courseBook.AddCourseBook(23, 37, books);
            courseBook.AddCourseBook(23, 38, books);
            courseBook.AddCourseBook(24, 39, books);
            courseBook.AddCourseBook(24, 40, books);
            courseBook.AddCourseBook(24, 41, books);
            courseBook.AddCourseBook(15, 42, books);
            courseBook.AddCourseBook(18, 43, books);
            courseBook.AddCourseBook(18, 44, books);
            courseBook.AddCourseBook(19, 45, books);
            courseBook.AddCourseBook(19, 46, books);
            courseBook.AddCourseBook(20, 47, books);
            courseBook.AddCourseBook(12, 48, books);
            courseBook.AddCourseBook(13, 48, books);
            courseBook.AddCourseBook(12, 49, books);
            courseBook.AddCourseBook(11, 50, books);
            courseBook.AddCourseBook(11, 51, books);
            courseBook.AddCourseBook(21, 52, books);
            courseBook.AddCourseBook(23, 53, books);
            courseBook.AddCourseBook(4, 54, books);
            courseBook.AddCourseBook(3, 55, books);
            courseBook.AddCourseBook(5, 56, books);
            courseBook.AddCourseBook(3, 57, books);
            courseBook.AddCourseBook(2, 58, books);
            courseBook.AddCourseBook(1, 59, books);
            courseBook.AddCourseBook(16, 60, books);
            courseBook.AddCourseBook(22, 61, books);
            courseBook.AddCourseBook(10, 62, books);
            courseBook.AddCourseBook(11, 63, books);
            courseBook.AddCourseBook(3, 64, books);
            courseBook.AddCourseBook(21, 65, books);
            courseBook.AddCourseBook(17, 66, books);
            courseBook.AddCourseBook(15, 67, books);
            courseBook.AddCourseBook(3, 68, books);
            courseBook.AddCourseBook(10, 69, books);
            courseBook.AddCourseBook(23, 70, books);
            courseBook.AddCourseBook(16, 71, books);
            courseBook.AddCourseBook(6, 72, books);
            courseBook.AddCourseBook(7, 73, books);
            courseBook.AddCourseBook(14, 74, books);
            courseBook.AddCourseBook(11, 75, books);
            courseBook.AddCourseBook(14, 76, books);
            courseBook.AddCourseBook(25, 77, books);
            courseBook.AddCourseBook(25, 78, books);
            courseBook.AddCourseBook(25, 79, books);
            courseBook.AddCourseBook(26, 80, books);
            courseBook.AddCourseBook(26, 81, books);
            courseBook.AddCourseBook(21, 82, books);
            courseBook.AddCourseBook(7, 83, books);
            courseBook.AddCourseBook(11, 84, books);
            courseBook.AddCourseBook(19, 85, books);
            courseBook.AddCourseBook(19, 86, books);
            courseBook.AddCourseBook(27, 87, books);
            courseBook.AddCourseBook(27, 88, books);
            courseBook.AddCourseBook(13, 89, books);
            courseBook.AddCourseBook(13, 90, books);
            courseBook.AddCourseBook(13, 91, books);
            courseBook.AddCourseBook(1, 92, books);
            courseBook.AddCourseBook(24, 93, books);

            modelBuilder.Entity<AcademicCourseBook>().HasData(courseBook);

            #endregion

            #region users

            List<User> users = new List<User>();

            // string datetime pour le lastLogin
            string lastLogin = DateTime.Now.ToString("s", CultureInfo.InvariantCulture);

            string password = "Password1";

            User user = new User
            {
                Id = 1,
                IsActive = true,
                Username = "user1",
                Email = "user1@mail.com",
                FirstName = "Georges",
                LastName = "Allard",
                CityId = 879,
                AcademicProgramId = 1,
                ImageCopyId = null,
                LastLogin = lastLogin
            };
            user.CreatePasswordHashSalt(password);
            users.Add(user);

            user = new User
            {
                Id = 2,
                IsActive = true,
                Username = "user2",
                Email = "user2@mail.com",
                FirstName = "Alain",
                LastName = "Michaud",
                CityId = 469,
                AcademicProgramId = 2,
                ImageCopyId = null,
                LastLogin = lastLogin
            };
            user.CreatePasswordHashSalt(password);
            users.Add(user);

            user = new User
            {
                Id = 3,
                IsActive = true,
                Username = "user3",
                Email = "user3@mail.com",
                FirstName = "Sylvie",
                LastName = "Lalonde",
                CityId = 235,
                AcademicProgramId = 4,
                ImageCopyId = null,
                LastLogin = lastLogin
            };
            user.CreatePasswordHashSalt(password);
            users.Add(user);

            user = new User
            {
                Id = 4,
                IsActive = true,
                Username = "user4",
                Email = "user4@mail.com",
                FirstName = "Bob",
                LastName = "Alphonseau",
                CityId = 980,
                AcademicProgramId = 3,
                ImageCopyId = null,
                LastLogin = lastLogin
            };
            user.CreatePasswordHashSalt(password);
            users.Add(user);

            user = new User
            {
                Id = 5,
                IsActive = true,
                Username = "user5",
                Email = "user5@mail.com",
                FirstName = "Michel",
                LastName = "Paiement",
                CityId = 436,
                AcademicProgramId = 1,
                ImageCopyId = null,
                LastLogin = lastLogin
            };
            user.CreatePasswordHashSalt(password);
            users.Add(user);

            user = new User
            {
                Id = 6,
                IsActive = true,
                Username = "user6",
                Email = "user6@mail.com",
                FirstName = "Pierette",
                LastName = "Tremblay",
                CityId = 780,
                AcademicProgramId = 1,
                ImageCopyId = null,
                LastLogin = lastLogin
            };
            user.CreatePasswordHashSalt(password);
            users.Add(user);

            user = new User
            {
                Id = 7,
                IsActive = true,
                Username = "user7",
                Email = "user7@mail.com",
                FirstName = "Marie",
                LastName = "Beaudoin",
                CityId = 657,
                ImageCopyId = null,
                LastLogin = lastLogin
            };
            user.CreatePasswordHashSalt(password);
            users.Add(user);

            user = new User
            {
                Id = 8,
                IsActive = true,
                Username = "user8",
                Email = "user8@mail.com",
                FirstName = "Pierre",
                LastName = "Letendre",
                CityId = 200,
                AcademicProgramId = 4,
                ImageCopyId = null,
                LastLogin = lastLogin
            };
            user.CreatePasswordHashSalt(password);
            users.Add(user);

            user = new User
            {
                Id = 9,
                IsActive = true,
                Username = "user9",
                Email = "user9@mail.com",
                FirstName = "André",
                LastName = "Giguère",
                CityId = 23,
                AcademicProgramId = 1,
                ImageCopyId = null,
                LastLogin = lastLogin
            };
            user.CreatePasswordHashSalt(password);
            users.Add(user);

            user = new User
            {
                Id = 10,
                IsActive = true,
                Username = "user10",
                Email = "user10@mail.com",
                FirstName = "Phillipe",
                LastName = "Noireux",
                CityId = 298,
                AcademicProgramId = 4,
                ImageCopyId = null,
                LastLogin = lastLogin
            };
            user.CreatePasswordHashSalt(password);
            users.Add(user);

            user = new User
            {
                Id = 11,
                IsActive = true,
                Username = "user11",
                Email = "user11@mail.com",
                FirstName = "Sophie",
                LastName = "LeDieux",
                CityId = 398,
                AcademicProgramId = 2,
                ImageCopyId = null,
                LastLogin = lastLogin
            };
            user.CreatePasswordHashSalt(password);
            users.Add(user);

            user = new User
            {
                Id = 12,
                IsActive = true,
                Username = "user12",
                Email = "user12@mail.com",
                FirstName = "André",
                LastName = "Deschamps",
                CityId = 487,
                AcademicProgramId = 3,
                ImageCopyId = null,
                LastLogin = lastLogin
            };
            user.CreatePasswordHashSalt(password);
            users.Add(user);

            modelBuilder.Entity<User>().HasData(users);

            #endregion

            #region cours-user

            List<AcademicCourseUser> courseUser = new List<AcademicCourseUser>();

            // si mode random
            if (RANDOM)
            {
                foreach(User u in users)
                {
                    // les cours du programme du user
                    List<int> userCourses = courseProgram.
                        Where(cp => cp.AcademicProgramId == u.AcademicProgramId).
                            Select(cp => cp.AcademicCourseId).ToList();

                    // nombre maximum de cours
                    int maxNumCourse = userCourses.Count > 15 ? userCourses.Count - 8 : userCourses.Count;

                    // nombre de cours ajoutés
                    int numCourses = rnd.Next(0, maxNumCourse);

                    for(int j = 0; j < numCourses; j++)
                    {
                        bool next = false;

                        // tant que c'est un doublon
                        while (!next)
                        {
                            int selectedIndex = rnd.Next(userCourses.Count);

                            // cours sélectionné
                            int selectedCourse = userCourses[selectedIndex];

                            // vérification du doublon
                            if(courseUser.Where(cu => cu.AcademicCourseId == selectedCourse &&
                                    cu.UserId == u.Id).ToList().Count == 0)
                            {
                                // ajout du cours et exit boucle while
                                courseUser.Add(new AcademicCourseUser { AcademicCourseId = selectedCourse, UserId = u.Id });
                                next = true;
                            }
                        }
                    }
                }
            }
            else
            {
                #region insert manuel

                courseUser.Add(new AcademicCourseUser { AcademicCourseId = 1, UserId = 1 });
                courseUser.Add(new AcademicCourseUser { AcademicCourseId = 2, UserId = 1 });
                courseUser.Add(new AcademicCourseUser { AcademicCourseId = 3, UserId = 1 });
                courseUser.Add(new AcademicCourseUser { AcademicCourseId = 4, UserId = 1 });
                courseUser.Add(new AcademicCourseUser { AcademicCourseId = 5, UserId = 1 });
                courseUser.Add(new AcademicCourseUser { AcademicCourseId = 6, UserId = 1 });
                courseUser.Add(new AcademicCourseUser { AcademicCourseId = 7, UserId = 1 });
                courseUser.Add(new AcademicCourseUser { AcademicCourseId = 8, UserId = 1 });
                courseUser.Add(new AcademicCourseUser { AcademicCourseId = 9, UserId = 1 });
                courseUser.Add(new AcademicCourseUser { AcademicCourseId = 10, UserId = 1 });
                courseUser.Add(new AcademicCourseUser { AcademicCourseId = 15, UserId = 1 });
                courseUser.Add(new AcademicCourseUser { AcademicCourseId = 16, UserId = 1 });
                courseUser.Add(new AcademicCourseUser { AcademicCourseId = 17, UserId = 1 });

                courseUser.Add(new AcademicCourseUser { AcademicCourseId = 1, UserId = 2 });
                courseUser.Add(new AcademicCourseUser { AcademicCourseId = 2, UserId = 2 });
                courseUser.Add(new AcademicCourseUser { AcademicCourseId = 3, UserId = 2 });
                courseUser.Add(new AcademicCourseUser { AcademicCourseId = 4, UserId = 2 });
                courseUser.Add(new AcademicCourseUser { AcademicCourseId = 5, UserId = 2 });
                courseUser.Add(new AcademicCourseUser { AcademicCourseId = 6, UserId = 2 });
                courseUser.Add(new AcademicCourseUser { AcademicCourseId = 7, UserId = 2 });

                courseUser.Add(new AcademicCourseUser { AcademicCourseId = 11, UserId = 3 });
                courseUser.Add(new AcademicCourseUser { AcademicCourseId = 12, UserId = 3 });
                courseUser.Add(new AcademicCourseUser { AcademicCourseId = 13, UserId = 3 });
                courseUser.Add(new AcademicCourseUser { AcademicCourseId = 14, UserId = 3 });

                courseUser.Add(new AcademicCourseUser { AcademicCourseId = 1, UserId = 5 });
                courseUser.Add(new AcademicCourseUser { AcademicCourseId = 2, UserId = 5 });
                courseUser.Add(new AcademicCourseUser { AcademicCourseId = 4, UserId = 5 });
                courseUser.Add(new AcademicCourseUser { AcademicCourseId = 5, UserId = 5 });
                courseUser.Add(new AcademicCourseUser { AcademicCourseId = 6, UserId = 5 });
                courseUser.Add(new AcademicCourseUser { AcademicCourseId = 9, UserId = 5 });
                courseUser.Add(new AcademicCourseUser { AcademicCourseId = 10, UserId = 5 });
                courseUser.Add(new AcademicCourseUser { AcademicCourseId = 17, UserId = 5 });

                courseUser.Add(new AcademicCourseUser { AcademicCourseId = 1, UserId = 6 });
                courseUser.Add(new AcademicCourseUser { AcademicCourseId = 6, UserId = 6 });
                courseUser.Add(new AcademicCourseUser { AcademicCourseId = 7, UserId = 6 });
                courseUser.Add(new AcademicCourseUser { AcademicCourseId = 8, UserId = 6 });
                courseUser.Add(new AcademicCourseUser { AcademicCourseId = 9, UserId = 6 });
                courseUser.Add(new AcademicCourseUser { AcademicCourseId = 10, UserId = 6 });
                courseUser.Add(new AcademicCourseUser { AcademicCourseId = 17, UserId = 6 });

                courseUser.Add(new AcademicCourseUser { AcademicCourseId = 7, UserId = 7 });
                courseUser.Add(new AcademicCourseUser { AcademicCourseId = 20, UserId = 7 });
                courseUser.Add(new AcademicCourseUser { AcademicCourseId = 21, UserId = 7 });
                courseUser.Add(new AcademicCourseUser { AcademicCourseId = 6, UserId = 7 });
                courseUser.Add(new AcademicCourseUser { AcademicCourseId = 13, UserId = 7 });
                courseUser.Add(new AcademicCourseUser { AcademicCourseId = 11, UserId = 7 });
                courseUser.Add(new AcademicCourseUser { AcademicCourseId = 18, UserId = 7 });

                #endregion
            }

            modelBuilder.Entity<AcademicCourseUser>().HasData(courseUser);

            #endregion

            #region livre-user

            List<BookUser> bookUser = new List<BookUser>();

            // si mode random
            if (RANDOM)
            {
                foreach (User u in users)
                {
                    // livres des cours ajoutés du user
                    List<int> userBooks = courseUser.
                        Where(cu => cu.UserId == u.Id).
                            Join(courseBook,
                                cu => cu.AcademicCourseId,
                                cb => cb.AcademicCourseId,
                                (cu, cb) => cb.BookId).ToList();

                    // nombre maximum de livres
                    int maxNumBooks = userBooks.Count > 10 ? userBooks.Count - 6: userBooks.Count;

                    // nombre de livres recherchés
                    int numBooks = rnd.Next(0, maxNumBooks);

                    for (int j = 0; j < numBooks; j++)
                    {
                        bool next = false;

                        // tant qu'on a un doublon
                        while (!next)
                        {
                            int selectedIndex = rnd.Next(userBooks.Count);

                            // livre sélectionné
                            int selectedBook = userBooks[selectedIndex];

                            // vérification du doublon
                            if (bookUser.Where(bu => bu.BookId == selectedBook &&
                                     bu.UserId == u.Id).ToList().Count == 0)
                            {
                                // ajout à la liste et exit boucle while
                                bookUser.Add(new BookUser { BookId = selectedBook, UserId = u.Id });
                                next = true;
                            }
                        }
                    }
                }
            }
            else
            {
                #region insert-manuel

                bookUser.Add(new BookUser { BookId = 17, UserId = 1 });
                bookUser.Add(new BookUser { BookId = 6, UserId = 1 });
                bookUser.Add(new BookUser { BookId = 23, UserId = 1 });
                bookUser.Add(new BookUser { BookId = 35, UserId = 1 });
                bookUser.Add(new BookUser { BookId = 9, UserId = 2 });
                bookUser.Add(new BookUser { BookId = 8, UserId = 2 });
                bookUser.Add(new BookUser { BookId = 26, UserId = 2 });
                bookUser.Add(new BookUser { BookId = 17, UserId = 2 });
                bookUser.Add(new BookUser { BookId = 29, UserId = 2 });
                bookUser.Add(new BookUser { BookId = 14, UserId = 2 });
                bookUser.Add(new BookUser { BookId = 15, UserId = 3 });
                bookUser.Add(new BookUser { BookId = 80, UserId = 3 });
                bookUser.Add(new BookUser { BookId = 84, UserId = 3 });
                bookUser.Add(new BookUser { BookId = 47, UserId = 3 });
                bookUser.Add(new BookUser { BookId = 21, UserId = 5 });
                bookUser.Add(new BookUser { BookId = 32, UserId = 5 });
                bookUser.Add(new BookUser { BookId = 12, UserId = 6 });
                bookUser.Add(new BookUser { BookId = 41, UserId = 7 });
                bookUser.Add(new BookUser { BookId = 22, UserId = 7 });
                bookUser.Add(new BookUser { BookId = 26, UserId = 7 });
                bookUser.Add(new BookUser { BookId = 19, UserId = 7 });
                bookUser.Add(new BookUser { BookId = 55, UserId = 8 });
                bookUser.Add(new BookUser { BookId = 68, UserId = 8 });
                bookUser.Add(new BookUser { BookId = 22, UserId = 8 });
                bookUser.Add(new BookUser { BookId = 43, UserId = 9 });
                bookUser.Add(new BookUser { BookId = 56, UserId = 9 });

                #endregion
            }

            modelBuilder.Entity<BookUser>().HasData(bookUser);

            #endregion

            #region exemplaires

            List<BookCopy> copies = new List<BookCopy>();

            // si mode random
            if (RANDOM)
            {
                // probabilités de l'état du livre
                int[] conditionProb = new int[] { 1, 2, 3, 4, 4, 5, 5, 6, 6, 6, 6, 7, 7, 7, 7, 7, 8, 8, 8, 8, 9, 9, 10 };

                string[] TYPES = new string[] { AppStrings.TransactionEchange, AppStrings.TransactionVenteEchange, AppStrings.TransactionVente };

                // probabilités du type de transaction
                int[] typeProb = new int[] { 0, 0, 1, 1, 2, 2, 2, 2, 2, 2 };

                for(int i = 1; i <= NUM_COPIES; i++)
                {
                    bool isChosen = false;

                    int bookId = 0;
                    Book b = null;

                    // tant qu'on a un livre avec
                    // le max d'exemplaires
                    while (!isChosen)
                    {
                        // livre sélectionné
                        bookId = rnd.Next(0, books.Count);
                        b = books[bookId];

                        // vérification max exemplaires du même livre
                        if(copies.Where(c => c.BookId == b.Id).ToList().Count < NB_MAX_COPIES)
                        {
                            isChosen = true;
                        }
                    }

                    isChosen = false;

                    // type de transaction
                    int typeId = rnd.Next(0, typeProb.Length);
                    string transaction = TYPES[typeProb[typeId]];

                    int condId = 0;
                    int condition = 0;

                    // tant qu'on a le max
                    // du même état pour les
                    // exemplaires de ce livre
                    while (!isChosen)
                    {
                        // condition sélectionnée
                        condId = rnd.Next(0, conditionProb.Length);
                        condition = conditionProb[condId];

                        // vérification max num condition
                        if (copies.Where(c => c.BookId == b.Id && c.Condition == condition).ToList().Count < NB_MAX_CONDITION)
                        {
                            isChosen = true;
                        }
                    }

                    isChosen = false;

                    // user
                    int userId = rnd.Next(1, users.Count + 1);

                    double p = 0;
                    int price = 0;
                    if(transaction != AppStrings.TransactionEchange)
                    {
                        // modification du prix en fonction
                        // de l'état du livre (entre 10% ety 110% du prix original)
                        int percent = 0;
                        while (!isChosen)
                        {
                            // porcentage = +/- 13% de la condition * 10
                            percent = condition * 10;
                            percent = rnd.Next(percent - 13, percent + 13);

                            // minimum de 15% du prix de référence
                            if(percent < 15)
                            {
                                percent = 15;
                            }

                            p = b.ReferencePrice * percent / 100;
                            price = Convert.ToInt32(p);

                            // vérification max num différent prix pour les exemplaires du mêm livre
                            if (copies.Where(c => c.BookId == b.Id && c.Price == price).ToList().Count < NB_MAX_PRICE)
                            {
                                isChosen = true;
                            }
                        }
                    }

                    // ajout de l'exemplaire à la liste
                    BookCopy bc = new BookCopy
                    {
                        Id = i,
                        BookId = b.Id,
                        UserId = userId,
                        Condition = condition,
                        Price = price,
                        ImageCopyId = b.Id,
                        TransactionType = transaction
                    };
                    copies.Add(bc);
                }
            }
            else
            {
                #region insert-manuel

                copies.Add(new BookCopy { Id = 1, BookId = 1, UserId = 1, Condition = 6, Price = 79.60, ImageCopyId = 1, TransactionType = AppStrings.TransactionVente });
                copies.Add(new BookCopy { Id = 2, BookId = 1, UserId = 2, Condition = 8, Price = 86.00, ImageCopyId = 1, TransactionType = AppStrings.TransactionVente });
                copies.Add(new BookCopy { Id = 3, BookId = 1, UserId = 3, Condition = 7, Price = 0, ImageCopyId = 1, TransactionType = AppStrings.TransactionEchange });
                copies.Add(new BookCopy { Id = 4, BookId = 2, UserId = 3, Condition = 2, Price = 24.00, ImageCopyId = 2, TransactionType = AppStrings.TransactionVenteEchange });
                copies.Add(new BookCopy { Id = 5, BookId = 2, UserId = 3, Condition = 5, Price = 46.50, ImageCopyId = 2, TransactionType = AppStrings.TransactionVente });
                copies.Add(new BookCopy { Id = 6, BookId = 3, UserId = 2, Condition = 8, Price = 27.90, ImageCopyId = 3, TransactionType = AppStrings.TransactionVenteEchange });
                copies.Add(new BookCopy { Id = 7, BookId = 3, UserId = 1, Condition = 10, Price = 67.00, ImageCopyId = 3, TransactionType = AppStrings.TransactionVente });
                copies.Add(new BookCopy { Id = 8, BookId = 3, UserId = 1, Condition = 6, Price = 0, ImageCopyId = 3, TransactionType = AppStrings.TransactionEchange });
                copies.Add(new BookCopy { Id = 9, BookId = 4, UserId = 2, Condition = 5, Price = 39.00, ImageCopyId = 4, TransactionType = AppStrings.TransactionVenteEchange });
                copies.Add(new BookCopy { Id = 10, BookId = 4, UserId = 3, Condition = 1, Price = 0, ImageCopyId = 4, TransactionType = AppStrings.TransactionEchange });
                copies.Add(new BookCopy { Id = 11, BookId = 5, UserId = 3, Condition = 3, Price = 0, ImageCopyId = 5, TransactionType = AppStrings.TransactionEchange });
                copies.Add(new BookCopy { Id = 12, BookId = 7, UserId = 2, Condition = 5, Price = 67.00, ImageCopyId = 7, TransactionType = AppStrings.TransactionVente });
                copies.Add(new BookCopy { Id = 13, BookId = 7, UserId = 1, Condition = 6, Price = 0, ImageCopyId = 7, TransactionType = AppStrings.TransactionEchange });
                copies.Add(new BookCopy { Id = 14, BookId = 8, UserId = 1, Condition = 5, Price = 46.00, ImageCopyId = 8, TransactionType = AppStrings.TransactionVenteEchange });
                copies.Add(new BookCopy { Id = 15, BookId = 9, UserId = 2, Condition = 4, Price = 0, ImageCopyId = 9, TransactionType = AppStrings.TransactionEchange });
                copies.Add(new BookCopy { Id = 16, BookId = 9, UserId = 3, Condition = 3, Price = 10.20, ImageCopyId = 9, TransactionType = AppStrings.TransactionVente });
                copies.Add(new BookCopy { Id = 17, BookId = 9, UserId = 2, Condition = 6, Price = 0, ImageCopyId = 9, TransactionType = AppStrings.TransactionEchange });
                copies.Add(new BookCopy { Id = 18, BookId = 10, UserId = 1, Condition = 9, Price = 0, ImageCopyId = 10, TransactionType = AppStrings.TransactionEchange });
                copies.Add(new BookCopy { Id = 19, BookId = 10, UserId = 2, Condition = 8, Price = 22.48, ImageCopyId = 10, TransactionType = AppStrings.TransactionVente });
                copies.Add(new BookCopy { Id = 20, BookId = 11, UserId = 1, Condition = 2, Price = 46.00, ImageCopyId = 11, TransactionType = AppStrings.TransactionVenteEchange });
                copies.Add(new BookCopy { Id = 21, BookId = 11, UserId = 3, Condition = 7, Price = 0, ImageCopyId = 11, TransactionType = AppStrings.TransactionEchange });
                copies.Add(new BookCopy { Id = 22, BookId = 12, UserId = 2, Condition = 3, Price = 90.88, ImageCopyId = 12, TransactionType = AppStrings.TransactionVente });
                copies.Add(new BookCopy { Id = 23, BookId = 13, UserId = 1, Condition = 8, Price = 65.00, ImageCopyId = 13, TransactionType = AppStrings.TransactionVenteEchange });
                copies.Add(new BookCopy { Id = 24, BookId = 13, UserId = 2, Condition = 7, Price = 68.00, ImageCopyId = 13, TransactionType = AppStrings.TransactionVente });
                copies.Add(new BookCopy { Id = 25, BookId = 14, UserId = 3, Condition = 6, Price = 0, ImageCopyId = 14, TransactionType = AppStrings.TransactionEchange });
                copies.Add(new BookCopy { Id = 26, BookId = 15, UserId = 2, Condition = 5, Price = 32.50, ImageCopyId = 15, TransactionType = AppStrings.TransactionVente });
                copies.Add(new BookCopy { Id = 27, BookId = 16, UserId = 1, Condition = 7, Price = 54.90, ImageCopyId = 16, TransactionType = AppStrings.TransactionVenteEchange });
                copies.Add(new BookCopy { Id = 28, BookId = 16, UserId = 3, Condition = 2, Price = 12.00, ImageCopyId = 16, TransactionType = AppStrings.TransactionVente });
                copies.Add(new BookCopy { Id = 29, BookId = 17, UserId = 2, Condition = 8, Price = 0, ImageCopyId = 17, TransactionType = AppStrings.TransactionEchange });
                copies.Add(new BookCopy { Id = 30, BookId = 17, UserId = 1, Condition = 9, Price = 23.90, ImageCopyId = 17, TransactionType = AppStrings.TransactionVente });
                copies.Add(new BookCopy { Id = 31, BookId = 18, UserId = 2, Condition = 5, Price = 67.00, ImageCopyId = 18, TransactionType = AppStrings.TransactionVente });
                copies.Add(new BookCopy { Id = 32, BookId = 19, UserId = 3, Condition = 7, Price = 0, ImageCopyId = 19, TransactionType = AppStrings.TransactionEchange });
                copies.Add(new BookCopy { Id = 33, BookId = 19, UserId = 2, Condition = 6, Price = 98.00, ImageCopyId = 19, TransactionType = AppStrings.TransactionVenteEchange });
                copies.Add(new BookCopy { Id = 34, BookId = 20, UserId = 1, Condition = 9, Price = 23.90, ImageCopyId = 20, TransactionType = AppStrings.TransactionVente });
                copies.Add(new BookCopy { Id = 35, BookId = 21, UserId = 2, Condition = 7, Price = 0, ImageCopyId = 21, TransactionType = AppStrings.TransactionEchange });
                copies.Add(new BookCopy { Id = 36, BookId = 22, UserId = 3, Condition = 6, Price = 26.00, ImageCopyId = 22, TransactionType = AppStrings.TransactionVente });
                copies.Add(new BookCopy { Id = 37, BookId = 23, UserId = 2, Condition = 10, Price = 45.00, ImageCopyId = 23, TransactionType = AppStrings.TransactionVenteEchange });
                copies.Add(new BookCopy { Id = 39, BookId = 25, UserId = 4, Condition = 7, Price = 0, ImageCopyId = 25, TransactionType = AppStrings.TransactionEchange });
                copies.Add(new BookCopy { Id = 40, BookId = 26, UserId = 3, Condition = 6, Price = 35.00, ImageCopyId = 26, TransactionType = AppStrings.TransactionVenteEchange });

                copies.Add(new BookCopy { Id = 41, BookId = 1, UserId = 3, Condition = 2, Price = 0, ImageCopyId = 1, TransactionType = AppStrings.TransactionEchange });
                copies.Add(new BookCopy { Id = 42, BookId = 1, UserId = 1, Condition = 3, Price = 6.90, ImageCopyId = 1, TransactionType = AppStrings.TransactionVente });
                copies.Add(new BookCopy { Id = 43, BookId = 1, UserId = 2, Condition = 4, Price = 0, ImageCopyId = 1, TransactionType = AppStrings.TransactionEchange });
                copies.Add(new BookCopy { Id = 44, BookId = 2, UserId = 4, Condition = 1, Price = 22.00, ImageCopyId = 2, TransactionType = AppStrings.TransactionVente });
                copies.Add(new BookCopy { Id = 46, BookId = 3, UserId = 2, Condition = 8, Price = 10.80, ImageCopyId = 3, TransactionType = AppStrings.TransactionVenteEchange });
                copies.Add(new BookCopy { Id = 47, BookId = 3, UserId = 1, Condition = 10, Price = 22.43, ImageCopyId = 3, TransactionType = AppStrings.TransactionVente });
                copies.Add(new BookCopy { Id = 48, BookId = 3, UserId = 3, Condition = 5, Price = 0, ImageCopyId = 3, TransactionType = AppStrings.TransactionEchange });
                copies.Add(new BookCopy { Id = 50, BookId = 4, UserId = 2, Condition = 3, Price = 0, ImageCopyId = 4, TransactionType = AppStrings.TransactionEchange });
                copies.Add(new BookCopy { Id = 52, BookId = 7, UserId = 3, Condition = 7, Price = 29.08, ImageCopyId = 7, TransactionType = AppStrings.TransactionVente });
                copies.Add(new BookCopy { Id = 53, BookId = 7, UserId = 2, Condition = 6, Price = 0, ImageCopyId = 7, TransactionType = AppStrings.TransactionEchange });
                copies.Add(new BookCopy { Id = 55, BookId = 9, UserId = 2, Condition = 9, Price = 0, ImageCopyId = 9, TransactionType = AppStrings.TransactionEchange });
                copies.Add(new BookCopy { Id = 57, BookId = 9, UserId = 1, Condition = 1, Price = 0, ImageCopyId = 9, TransactionType = AppStrings.TransactionEchange });
                copies.Add(new BookCopy { Id = 59, BookId = 10, UserId = 2, Condition = 3, Price = 7.99, ImageCopyId = 10, TransactionType = AppStrings.TransactionVente });
                copies.Add(new BookCopy { Id = 61, BookId = 11, UserId = 1, Condition = 9, Price = 0, ImageCopyId = 11, TransactionType = AppStrings.TransactionEchange });
                copies.Add(new BookCopy { Id = 62, BookId = 12, UserId = 1, Condition = 4, Price = 0, ImageCopyId = 12, TransactionType = AppStrings.TransactionEchange });
                copies.Add(new BookCopy { Id = 63, BookId = 13, UserId = 1, Condition = 2, Price = 43.90, ImageCopyId = 13, TransactionType = AppStrings.TransactionVenteEchange });
                copies.Add(new BookCopy { Id = 65, BookId = 14, UserId = 2, Condition = 2, Price = 0, ImageCopyId = 14, TransactionType = AppStrings.TransactionEchange });
                copies.Add(new BookCopy { Id = 66, BookId = 15, UserId = 4, Condition = 4, Price = 10.00, ImageCopyId = 15, TransactionType = AppStrings.TransactionVenteEchange });
                copies.Add(new BookCopy { Id = 68, BookId = 16, UserId = 2, Condition = 8, Price = 14.55, ImageCopyId = 16, TransactionType = AppStrings.TransactionVente });
                copies.Add(new BookCopy { Id = 69, BookId = 17, UserId = 1, Condition = 5, Price = 0, ImageCopyId = 17, TransactionType = AppStrings.TransactionEchange });
                copies.Add(new BookCopy { Id = 71, BookId = 18, UserId = 1, Condition = 3, Price = 20.00, ImageCopyId = 18, TransactionType = AppStrings.TransactionEchange });
                copies.Add(new BookCopy { Id = 73, BookId = 19, UserId = 3, Condition = 8, Price = 76.90, ImageCopyId = 19, TransactionType = AppStrings.TransactionVenteEchange });
                copies.Add(new BookCopy { Id = 74, BookId = 20, UserId = 4, Condition = 7, Price = 21.90, ImageCopyId = 20, TransactionType = AppStrings.TransactionVente });
                copies.Add(new BookCopy { Id = 76, BookId = 22, UserId = 1, Condition = 4, Price = 76.90, ImageCopyId = 22, TransactionType = AppStrings.TransactionVente });
                copies.Add(new BookCopy { Id = 77, BookId = 23, UserId = 2, Condition = 10, Price = 67.00, ImageCopyId = 23, TransactionType = AppStrings.TransactionVenteEchange });
                copies.Add(new BookCopy { Id = 80, BookId = 26, UserId = 4, Condition = 9, Price = 61.75, ImageCopyId = 26, TransactionType = AppStrings.TransactionVenteEchange });

                copies.Add(new BookCopy { Id = 81, BookId = 27, UserId = 1, Condition = 8, Price = 0, ImageCopyId = 27, TransactionType = AppStrings.TransactionEchange });
                copies.Add(new BookCopy { Id = 82, BookId = 28, UserId = 1, Condition = 4, Price = 20, ImageCopyId = 28, TransactionType = AppStrings.TransactionVente });
                copies.Add(new BookCopy { Id = 83, BookId = 28, UserId = 2, Condition = 6, Price = 32, ImageCopyId = 28, TransactionType = AppStrings.TransactionVenteEchange });
                copies.Add(new BookCopy { Id = 84, BookId = 29, UserId = 4, Condition = 7, Price = 0, ImageCopyId = 29, TransactionType = AppStrings.TransactionEchange });
                copies.Add(new BookCopy { Id = 85, BookId = 30, UserId = 3, Condition = 1, Price = 41, ImageCopyId = 30, TransactionType = AppStrings.TransactionVenteEchange });
                copies.Add(new BookCopy { Id = 86, BookId = 30, UserId = 5, Condition = 5, Price = 56, ImageCopyId = 30, TransactionType = AppStrings.TransactionVente });
                copies.Add(new BookCopy { Id = 87, BookId = 31, UserId = 6, Condition = 7, Price = 0, ImageCopyId = 31, TransactionType = AppStrings.TransactionEchange });
                copies.Add(new BookCopy { Id = 88, BookId = 32, UserId = 2, Condition = 5, Price = 89, ImageCopyId = 32, TransactionType = AppStrings.TransactionVenteEchange });
                copies.Add(new BookCopy { Id = 89, BookId = 32, UserId = 3, Condition = 10, Price = 54, ImageCopyId = 32, TransactionType = AppStrings.TransactionVente });
                copies.Add(new BookCopy { Id = 90, BookId = 33, UserId = 1, Condition = 9, Price = 0, ImageCopyId = 33, TransactionType = AppStrings.TransactionEchange });
                copies.Add(new BookCopy { Id = 91, BookId = 34, UserId = 4, Condition = 6, Price = 99, ImageCopyId = 34, TransactionType = AppStrings.TransactionVenteEchange });
                copies.Add(new BookCopy { Id = 92, BookId = 34, UserId = 2, Condition = 7, Price = 20, ImageCopyId = 34, TransactionType = AppStrings.TransactionVente });
                copies.Add(new BookCopy { Id = 93, BookId = 35, UserId = 3, Condition = 4, Price = 21, ImageCopyId = 35, TransactionType = AppStrings.TransactionVenteEchange });
                copies.Add(new BookCopy { Id = 94, BookId = 35, UserId = 3, Condition = 7, Price = 12, ImageCopyId = 35, TransactionType = AppStrings.TransactionVenteEchange });
                copies.Add(new BookCopy { Id = 95, BookId = 35, UserId = 4, Condition = 8, Price = 0, ImageCopyId = 35, TransactionType = AppStrings.TransactionEchange });
                copies.Add(new BookCopy { Id = 96, BookId = 36, UserId = 1, Condition = 7, Price = 32, ImageCopyId = 36, TransactionType = AppStrings.TransactionVenteEchange });
                copies.Add(new BookCopy { Id = 97, BookId = 36, UserId = 2, Condition = 6, Price = 45, ImageCopyId = 36, TransactionType = AppStrings.TransactionVente });
                copies.Add(new BookCopy { Id = 98, BookId = 37, UserId = 3, Condition = 5, Price = 0, ImageCopyId = 37, TransactionType = AppStrings.TransactionEchange });
                copies.Add(new BookCopy { Id = 99, BookId = 38, UserId = 4, Condition = 8, Price = 65, ImageCopyId = 38, TransactionType = AppStrings.TransactionVenteEchange });
                copies.Add(new BookCopy { Id = 100, BookId = 38, UserId = 5, Condition = 7, Price = 32, ImageCopyId = 38, TransactionType = AppStrings.TransactionVente });

                copies.Add(new BookCopy { Id = 101, BookId = 27, UserId = 2, Condition = 2, Price = 0, ImageCopyId = 27, TransactionType = AppStrings.TransactionEchange });
                copies.Add(new BookCopy { Id = 102, BookId = 28, UserId = 2, Condition = 5, Price = 29, ImageCopyId = 28, TransactionType = AppStrings.TransactionVente });
                copies.Add(new BookCopy { Id = 104, BookId = 29, UserId = 5, Condition = 9, Price = 45, ImageCopyId = 29, TransactionType = AppStrings.TransactionVente });
                copies.Add(new BookCopy { Id = 106, BookId = 30, UserId = 6, Condition = 3, Price = 32, ImageCopyId = 30, TransactionType = AppStrings.TransactionVente });
                copies.Add(new BookCopy { Id = 107, BookId = 31, UserId = 5, Condition = 7, Price = 58, ImageCopyId = 31, TransactionType = AppStrings.TransactionVente });
                copies.Add(new BookCopy { Id = 109, BookId = 32, UserId = 2, Condition = 8, Price = 54, ImageCopyId = 32, TransactionType = AppStrings.TransactionVente });
                copies.Add(new BookCopy { Id = 111, BookId = 34, UserId = 2, Condition = 5, Price = 43, ImageCopyId = 34, TransactionType = AppStrings.TransactionVenteEchange });
                copies.Add(new BookCopy { Id = 112, BookId = 34, UserId = 5, Condition = 7, Price = 45, ImageCopyId = 34, TransactionType = AppStrings.TransactionVente });
                copies.Add(new BookCopy { Id = 113, BookId = 35, UserId = 6, Condition = 8, Price = 0, ImageCopyId = 35, TransactionType = AppStrings.TransactionEchange });
                copies.Add(new BookCopy { Id = 115, BookId = 35, UserId = 3, Condition = 2, Price = 50, ImageCopyId = 35, TransactionType = AppStrings.TransactionVente });
                copies.Add(new BookCopy { Id = 116, BookId = 36, UserId = 2, Condition = 8, Price = 71, ImageCopyId = 36, TransactionType = AppStrings.TransactionVenteEchange });
                copies.Add(new BookCopy { Id = 117, BookId = 36, UserId = 1, Condition = 4, Price = 35, ImageCopyId = 36, TransactionType = AppStrings.TransactionVente });
                copies.Add(new BookCopy { Id = 119, BookId = 38, UserId = 3, Condition = 6, Price = 54, ImageCopyId = 38, TransactionType = AppStrings.TransactionVenteEchange });

                copies.Add(new BookCopy { Id = 120, BookId = 39, UserId = 1, Condition = 2, Price = 19, ImageCopyId = 39, TransactionType = AppStrings.TransactionVente });
                copies.Add(new BookCopy { Id = 121, BookId = 39, UserId = 3, Condition = 4, Price = 48, ImageCopyId = 39, TransactionType = AppStrings.TransactionVenteEchange });
                copies.Add(new BookCopy { Id = 122, BookId = 40, UserId = 5, Condition = 6, Price = 56, ImageCopyId = 40, TransactionType = AppStrings.TransactionVente });
                copies.Add(new BookCopy { Id = 123, BookId = 41, UserId = 7, Condition = 6, Price = 0, ImageCopyId = 41, TransactionType = AppStrings.TransactionEchange });
                copies.Add(new BookCopy { Id = 124, BookId = 42, UserId = 9, Condition = 8, Price = 41, ImageCopyId = 42, TransactionType = AppStrings.TransactionVenteEchange });
                copies.Add(new BookCopy { Id = 125, BookId = 42, UserId = 2, Condition = 10, Price = 70, ImageCopyId = 42, TransactionType = AppStrings.TransactionVente });
                copies.Add(new BookCopy { Id = 126, BookId = 43, UserId = 4, Condition = 1, Price = 0, ImageCopyId = 43, TransactionType = AppStrings.TransactionEchange });
                copies.Add(new BookCopy { Id = 127, BookId = 44, UserId = 6, Condition = 3, Price = 50, ImageCopyId = 44, TransactionType = AppStrings.TransactionVenteEchange });
                copies.Add(new BookCopy { Id = 128, BookId = 44, UserId = 8, Condition = 5, Price = 32, ImageCopyId = 44, TransactionType = AppStrings.TransactionVente });
                copies.Add(new BookCopy { Id = 129, BookId = 45, UserId = 4, Condition = 7, Price = 66, ImageCopyId = 45, TransactionType = AppStrings.TransactionVenteEchange });
                copies.Add(new BookCopy { Id = 130, BookId = 46, UserId = 1, Condition = 3, Price = 0, ImageCopyId = 46, TransactionType = AppStrings.TransactionEchange });
                copies.Add(new BookCopy { Id = 131, BookId = 47, UserId = 3, Condition = 5, Price = 39, ImageCopyId = 47, TransactionType = AppStrings.TransactionVenteEchange });
                copies.Add(new BookCopy { Id = 132, BookId = 47, UserId = 5, Condition = 7, Price = 60, ImageCopyId = 47, TransactionType = AppStrings.TransactionVente });
                copies.Add(new BookCopy { Id = 133, BookId = 48, UserId = 7, Condition = 7, Price = 0, ImageCopyId = 48, TransactionType = AppStrings.TransactionEchange });
                copies.Add(new BookCopy { Id = 134, BookId = 48, UserId = 9, Condition = 9, Price = 12, ImageCopyId = 48, TransactionType = AppStrings.TransactionVenteEchange });
                copies.Add(new BookCopy { Id = 135, BookId = 39, UserId = 2, Condition = 10, Price = 23, ImageCopyId = 39, TransactionType = AppStrings.TransactionVente });
                copies.Add(new BookCopy { Id = 136, BookId = 41, UserId = 4, Condition = 9, Price = 0, ImageCopyId = 41, TransactionType = AppStrings.TransactionEchange });
                copies.Add(new BookCopy { Id = 137, BookId = 45, UserId = 6, Condition = 5, Price = 59, ImageCopyId = 45, TransactionType = AppStrings.TransactionVenteEchange });
                copies.Add(new BookCopy { Id = 138, BookId = 45, UserId = 8, Condition = 7, Price = 40, ImageCopyId = 45, TransactionType = AppStrings.TransactionVente });
                copies.Add(new BookCopy { Id = 139, BookId = 40, UserId = 4, Condition = 9, Price = 88, ImageCopyId = 40, TransactionType = AppStrings.TransactionVenteEchange });

                copies.Add(new BookCopy { Id = 140, BookId = 49, UserId = 1, Condition = 2, Price = 17, ImageCopyId = 49, TransactionType = AppStrings.TransactionVente });
                copies.Add(new BookCopy { Id = 141, BookId = 49, UserId = 3, Condition = 4, Price = 32, ImageCopyId = 49, TransactionType = AppStrings.TransactionVenteEchange });
                copies.Add(new BookCopy { Id = 142, BookId = 50, UserId = 5, Condition = 6, Price = 50, ImageCopyId = 50, TransactionType = AppStrings.TransactionVente });
                copies.Add(new BookCopy { Id = 143, BookId = 51, UserId = 7, Condition = 6, Price = 0, ImageCopyId = 51, TransactionType = AppStrings.TransactionEchange });
                copies.Add(new BookCopy { Id = 144, BookId = 52, UserId = 9, Condition = 8, Price = 70, ImageCopyId = 52, TransactionType = AppStrings.TransactionVenteEchange });
                copies.Add(new BookCopy { Id = 145, BookId = 52, UserId = 2, Condition = 10, Price = 79, ImageCopyId = 52, TransactionType = AppStrings.TransactionVente });
                copies.Add(new BookCopy { Id = 146, BookId = 53, UserId = 4, Condition = 1, Price = 0, ImageCopyId = 53, TransactionType = AppStrings.TransactionEchange });
                copies.Add(new BookCopy { Id = 147, BookId = 54, UserId = 6, Condition = 3, Price = 54, ImageCopyId = 54, TransactionType = AppStrings.TransactionVenteEchange });
                copies.Add(new BookCopy { Id = 148, BookId = 54, UserId = 8, Condition = 5, Price = 55, ImageCopyId = 54, TransactionType = AppStrings.TransactionVente });
                copies.Add(new BookCopy { Id = 149, BookId = 55, UserId = 4, Condition = 7, Price = 21, ImageCopyId = 55, TransactionType = AppStrings.TransactionVenteEchange });
                copies.Add(new BookCopy { Id = 150, BookId = 56, UserId = 1, Condition = 3, Price = 0, ImageCopyId = 56, TransactionType = AppStrings.TransactionEchange });
                copies.Add(new BookCopy { Id = 151, BookId = 57, UserId = 3, Condition = 5, Price = 6, ImageCopyId = 57, TransactionType = AppStrings.TransactionVenteEchange });
                copies.Add(new BookCopy { Id = 152, BookId = 57, UserId = 5, Condition = 7, Price = 78, ImageCopyId = 57, TransactionType = AppStrings.TransactionVente });
                copies.Add(new BookCopy { Id = 153, BookId = 58, UserId = 7, Condition = 7, Price = 0, ImageCopyId = 58, TransactionType = AppStrings.TransactionEchange });
                copies.Add(new BookCopy { Id = 154, BookId = 58, UserId = 9, Condition = 9, Price = 24, ImageCopyId = 58, TransactionType = AppStrings.TransactionVenteEchange });
                copies.Add(new BookCopy { Id = 155, BookId = 49, UserId = 2, Condition = 10, Price = 45, ImageCopyId = 49, TransactionType = AppStrings.TransactionVente });
                copies.Add(new BookCopy { Id = 156, BookId = 51, UserId = 4, Condition = 9, Price = 0, ImageCopyId = 51, TransactionType = AppStrings.TransactionEchange });
                copies.Add(new BookCopy { Id = 157, BookId = 55, UserId = 6, Condition = 5, Price = 50, ImageCopyId = 55, TransactionType = AppStrings.TransactionVenteEchange });
                copies.Add(new BookCopy { Id = 158, BookId = 55, UserId = 8, Condition = 7, Price = 71, ImageCopyId = 55, TransactionType = AppStrings.TransactionVente });
                copies.Add(new BookCopy { Id = 159, BookId = 50, UserId = 4, Condition = 9, Price = 34, ImageCopyId = 50, TransactionType = AppStrings.TransactionVenteEchange });

                copies.Add(new BookCopy { Id = 160, BookId = 59, UserId = 1, Condition = 2, Price = 19, ImageCopyId = 59, TransactionType = AppStrings.TransactionVente });
                copies.Add(new BookCopy { Id = 161, BookId = 59, UserId = 3, Condition = 4, Price = 23, ImageCopyId = 59, TransactionType = AppStrings.TransactionVenteEchange });
                copies.Add(new BookCopy { Id = 162, BookId = 60, UserId = 5, Condition = 6, Price = 54, ImageCopyId = 60, TransactionType = AppStrings.TransactionVente });
                copies.Add(new BookCopy { Id = 163, BookId = 61, UserId = 7, Condition = 6, Price = 0, ImageCopyId = 61, TransactionType = AppStrings.TransactionEchange });
                copies.Add(new BookCopy { Id = 164, BookId = 62, UserId = 9, Condition = 8, Price = 30, ImageCopyId = 62, TransactionType = AppStrings.TransactionVenteEchange });
                copies.Add(new BookCopy { Id = 165, BookId = 62, UserId = 2, Condition = 10, Price = 88, ImageCopyId = 62, TransactionType = AppStrings.TransactionVente });
                copies.Add(new BookCopy { Id = 166, BookId = 63, UserId = 4, Condition = 1, Price = 0, ImageCopyId = 63, TransactionType = AppStrings.TransactionEchange });
                copies.Add(new BookCopy { Id = 167, BookId = 64, UserId = 6, Condition = 3, Price = 50, ImageCopyId = 64, TransactionType = AppStrings.TransactionVenteEchange });
                copies.Add(new BookCopy { Id = 168, BookId = 64, UserId = 8, Condition = 8, Price = 38, ImageCopyId = 64, TransactionType = AppStrings.TransactionVente });
                copies.Add(new BookCopy { Id = 169, BookId = 65, UserId = 4, Condition = 7, Price = 10, ImageCopyId = 65, TransactionType = AppStrings.TransactionVenteEchange });
                copies.Add(new BookCopy { Id = 170, BookId = 66, UserId = 1, Condition = 3, Price = 0, ImageCopyId = 66, TransactionType = AppStrings.TransactionEchange });
                copies.Add(new BookCopy { Id = 171, BookId = 67, UserId = 3, Condition = 5, Price = 90, ImageCopyId = 67, TransactionType = AppStrings.TransactionVenteEchange });
                copies.Add(new BookCopy { Id = 172, BookId = 67, UserId = 5, Condition = 2, Price = 30, ImageCopyId = 67, TransactionType = AppStrings.TransactionVente });
                copies.Add(new BookCopy { Id = 173, BookId = 68, UserId = 7, Condition = 7, Price = 0, ImageCopyId = 68, TransactionType = AppStrings.TransactionEchange });
                copies.Add(new BookCopy { Id = 174, BookId = 68, UserId = 9, Condition = 9, Price = 67, ImageCopyId = 68, TransactionType = AppStrings.TransactionVenteEchange });
                copies.Add(new BookCopy { Id = 175, BookId = 59, UserId = 2, Condition = 10, Price = 40, ImageCopyId = 59, TransactionType = AppStrings.TransactionVente });
                copies.Add(new BookCopy { Id = 176, BookId = 61, UserId = 4, Condition = 4, Price = 0, ImageCopyId = 61, TransactionType = AppStrings.TransactionEchange });
                copies.Add(new BookCopy { Id = 177, BookId = 65, UserId = 6, Condition = 5, Price = 55, ImageCopyId = 65, TransactionType = AppStrings.TransactionVenteEchange });
                copies.Add(new BookCopy { Id = 178, BookId = 65, UserId = 8, Condition = 7, Price = 30, ImageCopyId = 65, TransactionType = AppStrings.TransactionVente });
                copies.Add(new BookCopy { Id = 179, BookId = 60, UserId = 4, Condition = 9, Price = 28, ImageCopyId = 60, TransactionType = AppStrings.TransactionVenteEchange });

                copies.Add(new BookCopy { Id = 180, BookId = 69, UserId = 1, Condition = 2, Price = 22, ImageCopyId = 69, TransactionType = AppStrings.TransactionVente });
                copies.Add(new BookCopy { Id = 181, BookId = 69, UserId = 3, Condition = 4, Price = 46, ImageCopyId = 69, TransactionType = AppStrings.TransactionVenteEchange });
                copies.Add(new BookCopy { Id = 182, BookId = 70, UserId = 5, Condition = 6, Price = 87, ImageCopyId = 70, TransactionType = AppStrings.TransactionVente });
                copies.Add(new BookCopy { Id = 183, BookId = 71, UserId = 7, Condition = 6, Price = 0, ImageCopyId = 71, TransactionType = AppStrings.TransactionEchange });
                copies.Add(new BookCopy { Id = 184, BookId = 72, UserId = 9, Condition = 8, Price = 20, ImageCopyId = 72, TransactionType = AppStrings.TransactionVenteEchange });
                copies.Add(new BookCopy { Id = 185, BookId = 72, UserId = 2, Condition = 10, Price = 40, ImageCopyId = 72, TransactionType = AppStrings.TransactionVente });
                copies.Add(new BookCopy { Id = 186, BookId = 73, UserId = 4, Condition = 1, Price = 0, ImageCopyId = 73, TransactionType = AppStrings.TransactionEchange });
                copies.Add(new BookCopy { Id = 187, BookId = 74, UserId = 6, Condition = 3, Price = 55, ImageCopyId = 74, TransactionType = AppStrings.TransactionVenteEchange });
                copies.Add(new BookCopy { Id = 188, BookId = 74, UserId = 8, Condition = 8, Price = 51, ImageCopyId = 74, TransactionType = AppStrings.TransactionVente });
                copies.Add(new BookCopy { Id = 189, BookId = 75, UserId = 4, Condition = 7, Price = 23, ImageCopyId = 75, TransactionType = AppStrings.TransactionVenteEchange });
                copies.Add(new BookCopy { Id = 190, BookId = 76, UserId = 1, Condition = 3, Price = 0, ImageCopyId = 76, TransactionType = AppStrings.TransactionEchange });

                // repeat
                copies.Add(new BookCopy { Id = 191, BookId = 39, UserId = 2, Condition = 2, Price = 38, ImageCopyId = 39, TransactionType = AppStrings.TransactionVenteEchange });
                copies.Add(new BookCopy { Id = 192, BookId = 40, UserId = 2, Condition = 4, Price = 0, ImageCopyId = 40, TransactionType = AppStrings.TransactionEchange });
                copies.Add(new BookCopy { Id = 194, BookId = 42, UserId = 3, Condition = 6, Price = 31, ImageCopyId = 42, TransactionType = AppStrings.TransactionVenteEchange });
                copies.Add(new BookCopy { Id = 195, BookId = 42, UserId = 4, Condition = 8, Price = 80, ImageCopyId = 42, TransactionType = AppStrings.TransactionVente });
                copies.Add(new BookCopy { Id = 197, BookId = 44, UserId = 7, Condition = 5, Price = 60, ImageCopyId = 44, TransactionType = AppStrings.TransactionVente });
                copies.Add(new BookCopy { Id = 198, BookId = 44, UserId = 7, Condition = 7, Price = 42, ImageCopyId = 44, TransactionType = AppStrings.TransactionVente });
                copies.Add(new BookCopy { Id = 199, BookId = 45, UserId = 8, Condition = 9, Price = 76, ImageCopyId = 45, TransactionType = AppStrings.TransactionVenteEchange });
                copies.Add(new BookCopy { Id = 200, BookId = 46, UserId = 9, Condition = 5, Price = 90, ImageCopyId = 46, TransactionType = AppStrings.TransactionVente });
                copies.Add(new BookCopy { Id = 202, BookId = 47, UserId = 3, Condition = 4, Price = 0, ImageCopyId = 47, TransactionType = AppStrings.TransactionEchange });
                copies.Add(new BookCopy { Id = 203, BookId = 48, UserId = 1, Condition = 4, Price = 34, ImageCopyId = 48, TransactionType = AppStrings.TransactionVente });
                copies.Add(new BookCopy { Id = 204, BookId = 48, UserId = 2, Condition = 10, Price = 22, ImageCopyId = 48, TransactionType = AppStrings.TransactionVenteEchange });
                copies.Add(new BookCopy { Id = 208, BookId = 45, UserId = 6, Condition = 8, Price = 30, ImageCopyId = 45, TransactionType = AppStrings.TransactionVente });
                copies.Add(new BookCopy { Id = 210, BookId = 49, UserId = 8, Condition = 4, Price = 27, ImageCopyId = 49, TransactionType = AppStrings.TransactionVenteEchange });
                copies.Add(new BookCopy { Id = 212, BookId = 50, UserId = 6, Condition = 7, Price = 49, ImageCopyId = 50, TransactionType = AppStrings.TransactionVente });
                copies.Add(new BookCopy { Id = 213, BookId = 51, UserId = 3, Condition = 7, Price = 0, ImageCopyId = 51, TransactionType = AppStrings.TransactionEchange });
                copies.Add(new BookCopy { Id = 214, BookId = 52, UserId = 4, Condition = 8, Price = 60, ImageCopyId = 52, TransactionType = AppStrings.TransactionVente });
                copies.Add(new BookCopy { Id = 217, BookId = 54, UserId = 2, Condition = 6, Price = 34, ImageCopyId = 54, TransactionType = AppStrings.TransactionVenteEchange });
                copies.Add(new BookCopy { Id = 218, BookId = 54, UserId = 6, Condition = 6, Price = 30, ImageCopyId = 54, TransactionType = AppStrings.TransactionVente });
                copies.Add(new BookCopy { Id = 220, BookId = 56, UserId = 2, Condition = 5, Price = 55, ImageCopyId = 56, TransactionType = AppStrings.TransactionVente });
                copies.Add(new BookCopy { Id = 221, BookId = 57, UserId = 7, Condition = 8, Price = 36, ImageCopyId = 57, TransactionType = AppStrings.TransactionVenteEchange });
                copies.Add(new BookCopy { Id = 222, BookId = 57, UserId = 6, Condition = 3, Price = 68, ImageCopyId = 57, TransactionType = AppStrings.TransactionVenteEchange });
                copies.Add(new BookCopy { Id = 225, BookId = 49, UserId = 3, Condition = 9, Price = 0, ImageCopyId = 49, TransactionType = AppStrings.TransactionEchange });
                copies.Add(new BookCopy { Id = 226, BookId = 55, UserId = 1, Condition = 6, Price = 30, ImageCopyId = 55, TransactionType = AppStrings.TransactionVente });
                copies.Add(new BookCopy { Id = 227, BookId = 50, UserId = 1, Condition = 8, Price = 25, ImageCopyId = 50, TransactionType = AppStrings.TransactionVenteEchange });
                copies.Add(new BookCopy { Id = 228, BookId = 59, UserId = 2, Condition = 1, Price = 39, ImageCopyId = 59, TransactionType = AppStrings.TransactionVente });
                copies.Add(new BookCopy { Id = 229, BookId = 59, UserId = 2, Condition = 5, Price = 33, ImageCopyId = 59, TransactionType = AppStrings.TransactionVenteEchange });
                copies.Add(new BookCopy { Id = 230, BookId = 62, UserId = 8, Condition = 9, Price = 0, ImageCopyId = 62, TransactionType = AppStrings.TransactionEchange });
                copies.Add(new BookCopy { Id = 231, BookId = 62, UserId = 1, Condition = 10, Price = 79, ImageCopyId = 62, TransactionType = AppStrings.TransactionVente });
                copies.Add(new BookCopy { Id = 232, BookId = 63, UserId = 3, Condition = 9, Price = 66, ImageCopyId = 63, TransactionType = AppStrings.TransactionVente });
                copies.Add(new BookCopy { Id = 233, BookId = 64, UserId = 7, Condition = 8, Price = 56, ImageCopyId = 64, TransactionType = AppStrings.TransactionVente });
                copies.Add(new BookCopy { Id = 234, BookId = 65, UserId = 3, Condition = 9, Price = 24, ImageCopyId = 65, TransactionType = AppStrings.TransactionVenteEchange });
                copies.Add(new BookCopy { Id = 235, BookId = 67, UserId = 4, Condition = 5, Price = 32, ImageCopyId = 67, TransactionType = AppStrings.TransactionVente });
                copies.Add(new BookCopy { Id = 236, BookId = 68, UserId = 6, Condition = 6, Price = 43, ImageCopyId = 68, TransactionType = AppStrings.TransactionVente });
                copies.Add(new BookCopy { Id = 237, BookId = 59, UserId = 1, Condition = 10, Price = 0, ImageCopyId = 59, TransactionType = AppStrings.TransactionEchange });
                copies.Add(new BookCopy { Id = 238, BookId = 61, UserId = 3, Condition = 6, Price = 32, ImageCopyId = 61, TransactionType = AppStrings.TransactionVente });
                copies.Add(new BookCopy { Id = 239, BookId = 65, UserId = 7, Condition = 8, Price = 38, ImageCopyId = 65, TransactionType = AppStrings.TransactionVenteEchange });
                copies.Add(new BookCopy { Id = 240, BookId = 69, UserId = 2, Condition = 4, Price = 34, ImageCopyId = 69, TransactionType = AppStrings.TransactionVente });
                copies.Add(new BookCopy { Id = 241, BookId = 69, UserId = 4, Condition = 9, Price = 40, ImageCopyId = 69, TransactionType = AppStrings.TransactionVenteEchange });
                copies.Add(new BookCopy { Id = 242, BookId = 70, UserId = 6, Condition = 5, Price = 0, ImageCopyId = 70, TransactionType = AppStrings.TransactionEchange });
                copies.Add(new BookCopy { Id = 243, BookId = 72, UserId = 3, Condition = 8, Price = 49, ImageCopyId = 72, TransactionType = AppStrings.TransactionVente });
                copies.Add(new BookCopy { Id = 244, BookId = 74, UserId = 7, Condition = 8, Price = 31, ImageCopyId = 74, TransactionType = AppStrings.TransactionVenteEchange });
                copies.Add(new BookCopy { Id = 245, BookId = 74, UserId = 9, Condition = 7, Price = 56, ImageCopyId = 74, TransactionType = AppStrings.TransactionVente });
                copies.Add(new BookCopy { Id = 246, BookId = 76, UserId = 2, Condition = 4, Price = 0, ImageCopyId = 76, TransactionType = AppStrings.TransactionEchange });
                #endregion
            }

            modelBuilder.Entity<BookCopy>().HasData(copies);

            #endregion

            base.OnModelCreating(modelBuilder);
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlite(@"Data Source=" + Path.Combine(AppContext.BaseDirectory, "database.db"));

            optionsBuilder.UseLazyLoadingProxies();

            base.OnConfiguring(optionsBuilder);
        }

        #region villes

        public List<string> cityNames = new List<string> {
            "Abercorn",
            "Acton Vale",
            "Adstock",
            "Aguanish",
            "Akulivik",
            "Akwesasne",
            "Albanel",
            "Albert",
            "Alleyn-et-Cawood",
            "Alma",
            "Amherst",
            "Amos",
            "Amqui",
            "Ange-Gardien",
            "Armagh",
            "Arundel",
            "Ascot Corner",
            "Aston-Jonction",
            "Auclair",
            "Audet",
            "Aumond",
            "Aupaluk",
            "Austin",
            "Authier",
            "Authier-Nord",
            "Ayer's Cliff",
            "Baie-Atibenne",
            "Baie-Comeau",
            "Baie-de-la-Bouteille",
            "Baie-des-Chaloupes",
            "Baie-des-Sables",
            "Baie-d'Hudson",
            "Baie-du-Febvre",
            "Baie-D'Urfé",
            "Baie-Johan-Beetz",
            "Baie-Obaoca",
            "Baie-Sainte-Catherine",
            "Baie-Saint-Paul",
            "Baie-Trinité",
            "Barkmere",
            "Barnston-Ouest",
            "Barraute",
            "Batiscan",
            "Beaconsfield",
            "Béarn",
            "Beauce",
            "Beauharnois",
            "Beaulac-Garthby",
            "Beaumont",
            "Beaupré",
            "Bécancour",
            "Bedford",
            "Bégin",
            "Belcourt",
            "Belle-Rivière",
            "Belleterre",
            "Belœil",
            "Berry",
            "Berthier-sur-Mer",
            "Berthier",
            "Béthanie",
            "Biencourt",
            "Blain",
            "Blanc-Sablon",
            "Blue Sea",
            "Boileau",
            "Boisbriand",
            "Boischatel",
            "Bois-des-Filion",
            "Bois-Franc",
            "Bolton-Est",
            "Bolton-Ouest",
            "Bonaventure",
            "Bonne-Espérance",
            "Bonsecours",
            "Boucher",
            "Bouchette",
            "Bowman",
            "Brébeuf",
            "Brigham",
            "Bristol",
            "Brome",
            "Bromont",
            "Brossard",
            "Brownsburg-Chatham",
            "Bryson",
            "Bury",
            "Cacouna",
            "Calixa-Lavallée",
            "Campbell's Bay",
            "Candiac",
            "Caniapiscau",
            "Cantley",
            "Cap-Chat",
            "Caplan",
            "Cap-Saint-Ignace",
            "Cap-Santé",
            "Carignan",
            "Carleton-sur-Mer",
            "Cascades-Malignes",
            "Cascapédia–Saint-Jules",
            "Causapscal",
            "Cayamant",
            "Chambly",
            "Chambord",
            "Champlain",
            "Champneuf",
            "Chandler",
            "Chapais",
            "Charette",
            "Charlemagne",
            "Chartier",
            "Châteauguay",
            "Château-Richer",
            "Chazel",
            "Chelsea",
            "Chéné",
            "Chertsey",
            "Chester",
            "Chibougamau",
            "Chichester",
            "Chisasibi",
            "Chute-aux-Outardes",
            "Chute-Saint-Philippe",
            "Clarendon",
            "Clermont",
            "Clerval",
            "Cleveland",
            "Cloridorme",
            "Coaticook",
            "Collines-du-Basque",
            "Colombier",
            "Compton",
            "Contrecœur",
            "Cookshire-Eaton",
            "Coteau-du-Lac",
            "Côte-Nord-du-Golfe-du-Saint-Laurent",
            "Côte-Saint-Luc",
            "Coucoucache",
            "Coulée-des-Adolphe",
            "Courcelles",
            "Cowans",
            "Crabtree",
            "Dan",
            "Daveluy",
            "Dégelis",
            "Déléage",
            "Delson",
            "Denholm",
            "Dépôt-Échouani",
            "Desbiens",
            "Deschaillons-sur-Saint-Laurent",
            "Deschambault-Grondines",
            "Deux-Montagnes",
            "Disraeli",
            "Dix",
            "Dolbeau-Mistassini",
            "Dollard-Des Ormeaux",
            "Doncaster",
            "Donnacona",
            "Dorval",
            "Dosquet",
            "Drummond",
            "Dudswell",
            "Duhamel",
            "Duhamel-Ouest",
            "Dundee",
            "Dunham",
            "Duparquet",
            "Dupuy",
            "Durham-Sud",
            "East Angus",
            "East Broughton",
            "East Farnham",
            "East Hereford",
            "Eastmain",
            "Eastman",
            "Eeyou Istchee Baie-James",
            "Egan-Sud",
            "Elgin",
            "Entrelacs",
            "Escuminac",
            "Esprit-Saint",
            "Essipit",
            "Estérel",
            "Farnham",
            "Fassett",
            "Ferland-et-Boilleau",
            "Ferme-Neuve",
            "Fermont",
            "Forest",
            "Fort-Coulonge",
            "Fortier",
            "Fossambault-sur-le-Lac",
            "Frampton",
            "Franklin",
            "Franquelin",
            "Frelighsburg",
            "Frontenac",
            "Fugère",
            "Gallichan",
            "Gaspé",
            "Gatineau",
            "Gesgapegiag",
            "Girard",
            "Godbout",
            "Godmanchester",
            "Gore",
            "Gracefield",
            "Granby",
            "Grande-Rivière",
            "Grandes-Piles",
            "Grande-Vallée",
            "Grand-Métis",
            "Grand-Remous",
            "Grand-Saint-Esprit",
            "Gren",
            "Gros-Mécatina",
            "Grosse-Île",
            "Grosses-Roches",
            "Guérin",
            "Ham-Nord",
            "Hampden",
            "Hampstead",
            "Ham-Sud",
            "Harrington",
            "Hatley",
            "Havelock",
            "Havre-Saint-Pierre",
            "Hébert",
            "Hemmingford",
            "Henry",
            "Héroux",
            "Hinchinbrooke",
            "Honfleur",
            "Hope",
            "Hope Town",
            "Howick",
            "Huberdeau",
            "Hudson",
            "Hunter's Point",
            "Huntingdon",
            "Inukjuak",
            "Inverness",
            "Irlande",
            "Ivry-sur-le-Lac",
            "Ivujivik",
            "Joliette",
            "Kahnawake",
            "Kamouraska",
            "Kanesatake",
            "Kangiqsualujjuaq",
            "Kangiqsujuaq",
            "Kangirsuk",
            "Kataskomiq",
            "Kawawachikamach",
            "Kazabazua",
            "Kebaowek",
            "Kiamika",
            "Kingsbury",
            "Kingsey Falls",
            "Kinnear's Mills",
            "Kipawa",
            "Kirkland",
            "Kitcisakik",
            "Kitigan Zibi",
            "Kuujjuaq",
            "Kuujjuarapik",
            "Labelle",
            "La Bostonnais",
            "Labrecque",
            "Lac-Achouakan",
            "Lac-Akonapwehikan",
            "Lac-à-la-Croix",
            "Lac-Alfred",
            "Lac-Ashuapmushuan",
            "Lac-au-Brochet",
            "Lac-au-Saumon",
            "Lac-aux-Sables",
            "Lac-Bazinet",
            "Lac-Beauport",
            "Lac-Blanc",
            "Lac-Boisbouscache",
            "Lac-Bouchette",
            "Lac-Boulé",
            "Lac-Brome",
            "Lac-Cabasta",
            "Lac-Casault",
            "Lac-Chicobi",
            "Lac-Croche",
            "Lac-De La Bidière",
            "Lac-Delage",
            "Lac-de-la-Maison-de-Pierre",
            "Lac-de-la-Pomme",
            "Lac-des-Aigles",
            "Lac-des-Dix-Milles",
            "Lac-des-Eaux-Mortes",
            "Lac-des-Écorces",
            "Lac-Despinassy",
            "Lac-des-Plages",
            "Lac-des-Seize-Îles",
            "Lac-Devenyns",
            "Lac-Douaire",
            "Lac-Drolet",
            "Lac-du-Cerf",
            "Lac-Duparquet",
            "Lac-du-Taureau",
            "Lac-Édouard",
            "Lac-Ernest",
            "Lac-Etchemin",
            "Lac-Frontière",
            "Lac-Granet",
            "Lac-Huron",
            "Lachute",
            "Lac-Jacques-Cartier",
            "Lac-Jérôme",
            "Lac-John",
            "Lac-Juillet",
            "Lac-Lapeyrère",
            "Lac-Legendre",
            "Lac-Lenôtre",
            "Lac-Marguerite",
            "Lac-Masketsi",
            "Lac-Matapédia",
            "Lac-Matawin",
            "Lac-Mégantic",
            "Lac-Metei",
            "Lac-Minaki",
            "Lac-Ministuk",
            "Lac-Moncouche",
            "Lac-Moselle",
            "Lac-Nilgaut",
            "Lac-Normand",
            "Lacolle",
            "La Conception",
            "La Corne",
            "Lac-Oscar",
            "Lac-Pikauba",
            "Lac-Poulin",
            "Lac-Pythonga",
            "Lac-Rapide",
            "Lac-Saguay",
            "Lac-Sainte-Marie",
            "Lac-Saint-Joseph",
            "Lac-Saint-Paul",
            "Lac-Santé",
            "Lac-Sergent",
            "Lac-Simon",
            "Lac-Supérieur",
            "Lac-Tremblant-Nord",
            "Lac-Vacher",
            "Lac-Wagwabika",
            "Lac-Walker",
            "La Doré",
            "La Durantaye",
            "Laforce",
            "La Guadeloupe",
            "Lalemant",
            "La Macaza",
            "La Malbaie",
            "Lamarche",
            "La Martre",
            "Lambton",
            "La Minerve",
            "La Morandière",
            "La Motte",
            "L'Ancienne-Lorette",
            "Landrienne",
            "L'Ange-Gardien",
            "Laniel",
            "Lanoraie",
            "L'Anse-Saint-Jean",
            "Lantier",
            "La Patrie",
            "La Pêche",
            "La Pocatière",
            "La Prairie",
            "La Présentation",
            "La Rédemption",
            "La Reine",
            "La Romaine",
            "Larouche",
            "La Sarre",
            "L'Ascension",
            "L'Ascension-de-Notre-Seigneur",
            "L'Ascension-de-Patapédia",
            "L'Assomption",
            "La Trinité-des-Monts",
            "Latulipe-et-Gaboury",
            "La Tuque",
            "Launay",
            "Laurier-Station",
            "Laurier",
            "Laval",
            "Lavaltrie",
            "L'Avenir",
            "Laverlochère-Angliers",
            "La Visitation-de-l'Île-Dupas",
            "La Visitation-de-Yamaska",
            "Lawrence",
            "Lebel-sur-Quévillon",
            "Leclerc",
            "Lefebvre",
            "Lejeune",
            "Lemieux",
            "L'Épiphanie",
            "Léry",
            "Les Bergeronnes",
            "Les Cèdres",
            "Les Coteaux",
            "Les Éboulements",
            "Les Escoumins",
            "Les Hauteurs",
            "Les Îles-de-la-Madeleine",
            "Les Lacs-du-Témiscamingue",
            "Les Méchins",
            "Lévis",
            "L'Île-Cadieux",
            "L'Île-d'Anticosti",
            "L'Île-Dorval",
            "L'Île-du-Grand-Calumet",
            "L'Île-Perrot",
            "Lingwick",
            "Linton",
            "L'Isle-aux-Allumettes",
            "L'Isle-aux-Coudres",
            "L'Islet",
            "L'Isle-Verte",
            "Listuguj",
            "Litchfield",
            "Lochaber",
            "Lochaber-Partie-Ouest",
            "Longue-Pointe-de-Mingan",
            "Longue-Rive",
            "Longueuil",
            "Lorraine",
            "Lorrain",
            "Lotbinière",
            "Louise",
            "Low",
            "Lyster",
            "Macamic",
            "Maddington Falls",
            "Magog",
            "Malartic",
            "Maliotenam",
            "Manawan",
            "Mande",
            "Maniwaki",
            "Manseau",
            "Mansfield-et-Pontefract",
            "Maria",
            "Maricourt",
            "Marie",
            "Marsoui",
            "Marston",
            "Martin",
            "Mascouche",
            "Mashteuiatsh",
            "Maskinongé",
            "Massue",
            "Matagami",
            "Matane",
            "Matapédia",
            "Matchi-Manitou",
            "Matimekosh",
            "Mayo",
            "McMaster",
            "Melbourne",
            "Mercier",
            "Messines",
            "Métabetchouan–Lac-à-la-Croix",
            "Métis-sur-Mer",
            "Milan",
            "Mille-Isles",
            "Mingan",
            "Mirabel",
            "Mistissini",
            "Moffet",
            "Mont-Albert",
            "Mont-Alexandre",
            "Mont-Apica",
            "Montcalm",
            "Mont-Carmel",
            "Montcerf-Lytton",
            "Montebello",
            "Mont-Élie",
            "Mont-Joli",
            "Mont-Laurier",
            "Montmagny",
            "Montpellier",
            "Montréal",
            "Montréal-Est",
            "Montréal-Ouest",
            "Mont-Royal",
            "Mont-Saint-Grégoire",
            "Mont-Saint-Hilaire",
            "Mont-Saint-Michel",
            "Mont-Saint-Pierre",
            "Mont-Tremblant",
            "Mont-Valin",
            "Morin-Heights",
            "Mulgrave-et-Derry",
            "Murdoch",
            "Namur",
            "Nantes",
            "Napier",
            "Natashquan",
            "Nédélec",
            "Nemaska",
            "Neu",
            "New Carlisle",
            "Newport",
            "New Richmond",
            "Nicolet",
            "Nominingue",
            "Normandin",
            "Normétal",
            "North Hatley",
            "Notre-Dame-Auxiliatrice-de-Buckland",
            "Notre-Dame-de-Bonsecours",
            "Notre-Dame-de-Ham",
            "Notre-Dame-de-la-Merci",
            "Notre-Dame-de-la-Paix",
            "Notre-Dame-de-la-Salette",
            "Notre-Dame-de-l'Île-Perrot",
            "Notre-Dame-de-Lorette",
            "Notre-Dame-de-Lourdes",
            "Notre-Dame-de-Montauban",
            "Notre-Dame-de-Pontmain",
            "Notre-Dame-des-Anges",
            "Notre-Dame-des-Bois",
            "Notre-Dame-des-Monts",
            "Notre-Dame-des-Neiges",
            "Notre-Dame-des-Pins",
            "Notre-Dame-des-Prairies",
            "Notre-Dame-des-Sept-Douleurs",
            "Notre-Dame-de-Stanbridge",
            "Notre-Dame-du-Bon-Conseil",
            "Notre-Dame-du-Laus",
            "Notre-Dame-du-Mont-Carmel",
            "Notre-Dame-du-Nord",
            "Notre-Dame-du-Portage",
            "Notre-Dame-du-Rosaire",
            "Notre-Dame-du-Sacré-Cœur-d'Issoudun",
            "Nouvelle",
            "Noyan",
            "Nutashkuan",
            "Obedjiwan",
            "Odanak",
            "Ogden",
            "Oka",
            "Orford",
            "Ormstown",
            "Otterburn Park",
            "Otter Lake",
            "Oujé-Bougoumou",
            "Packington",
            "Padoue",
            "Pakuashipi",
            "Palmarolle",
            "Papineau",
            "Paris",
            "Paspébiac",
            "Passes-Dangereuses",
            "Percé",
            "Péribonka",
            "Pessamit",
            "Petite-Rivière-Saint-François",
            "Petite-Vallée",
            "Petit-Lac-Sainte-Anne",
            "Petit-Mécatina",
            "Petit-Saguenay",
            "Picard",
            "Piedmont",
            "Pierre",
            "Pike River",
            "Pikogan",
            "Pincourt",
            "Piopolis",
            "Plaisance",
            "Plessis",
            "Pohénégamook",
            "Pointe-à-la-Croix",
            "Pointe-aux-Outardes",
            "Pointe-Calumet",
            "Pointe-Claire",
            "Pointe-des-Cascades",
            "Pointe-Fortune",
            "Pointe-Lebel",
            "Pontiac",
            "Pont-Rouge",
            "Portage-du-Fort",
            "Port-Cartier",
            "Port-Daniel–Gascons",
            "Portneuf",
            "Portneuf-sur-Mer",
            "Potton",
            "Poularies",
            "Preissac",
            "Prévost",
            "Price",
            "Prince",
            "Puvirnituq",
            "Quaqtaq",
            "Québec",
            "Racine",
            "Ragueneau",
            "Rapide-Danseur",
            "Rapides-des-Joachims",
            "Rawdon",
            "Rémigny",
            "Repentigny",
            "Réservoir-Dozois",
            "Richelieu",
            "Richmond",
            "Rigaud",
            "Rimouski",
            "Ripon",
            "Ristigouche-Partie-Sud-Est",
            "Rivière-à-Claude",
            "Rivière-à-Pierre",
            "Rivière-au-Tonnerre",
            "Rivière-aux-Outardes",
            "Rivière-Beaudette",
            "Rivière-Bleue",
            "Rivière-Bonaventure",
            "Rivière-Bonjour",
            "Rivière-de-la-Savane",
            "Rivière-du-Loup",
            "Rivière-Éternité",
            "Rivière-Héva",
            "Rivière-Koksoak",
            "Rivière-Mistassini",
            "Rivière-Mouchalagane",
            "Rivière-Nipissis",
            "Rivière-Nouvelle",
            "Rivière-Ojima",
            "Rivière-Ouelle",
            "Rivière-Patapédia-Est",
            "Rivière-Rouge",
            "Rivière-Saint-Jean",
            "Rivière-Vaseuse",
            "Roberval",
            "Rochebaucourt",
            "Roquemaure",
            "Rosemère",
            "Rougemont",
            "Routhier",
            "Rouyn-Noranda",
            "Roxton",
            "Roxton Falls",
            "Roxton Pond",
            "Ruisseau-des-Mineurs",
            "Ruisseau-Ferguson",
            "Sacré-Cœur",
            "Sacré-Cœur-de-Jésus",
            "Sagard",
            "Saguenay",
            "Saint-Adalbert",
            "Saint-Adelme",
            "Saint-Adelphe",
            "Saint-Adolphe-d'Howard",
            "Saint-Adrien",
            "Saint-Adrien-d'Irlande",
            "Saint-Agapit",
            "Saint-Aimé",
            "Saint-Aimé-des-Lacs",
            "Saint-Aimé-du-Lac-des-Îles",
            "Saint-Alban",
            "Saint-Albert",
            "Saint-Alexandre",
            "Saint-Alexandre-de-Kamouraska",
            "Saint-Alexandre-des-Lacs",
            "Saint-Alexis",
            "Saint-Alexis-de-Matapédia",
            "Saint-Alexis-des-Monts",
            "Saint-Alfred",
            "Saint-Alphonse",
            "Saint-Alphonse-de-Granby",
            "Saint-Alphonse-Rodriguez",
            "Saint-Amable",
            "Saint-Ambroise",
            "Saint-Ambroise-de-Kildare",
            "Saint-Anaclet-de-Lessard",
            "Saint-André-Avellin",
            "Saint-André-d'Argenteuil",
            "Saint-André-de-Kamouraska",
            "Saint-André-de-Restigouche",
            "Saint-André-du-Lac-Saint-Jean",
            "Saint-Anicet",
            "Saint-Anselme",
            "Saint-Antoine-de-l'Isle-aux-Grues",
            "Saint-Antoine-de-Tilly",
            "Saint-Antoine-sur-Richelieu",
            "Saint-Antonin",
            "Saint-Apollinaire",
            "Saint-Armand",
            "Saint-Arsène",
            "Saint-Athanase",
            "Saint-Aubert",
            "Saint-Augustin",
            "Saint-Augustin-de-Desmaures",
            "Saint-Augustin-de-Woburn",
            "Saint-Barnabé",
            "Saint-Barnabé-Sud",
            "Saint-Barthélemy",
            "Saint-Basile",
            "Saint-Basile-le-Grand",
            "Saint-Benjamin",
            "Saint-Benoît-du-Lac",
            "Saint-Benoît-Labre",
            "Saint-Bernard",
            "Saint-Bernard-de-Lacolle",
            "Saint-Bernard-de-Michaud",
            "Saint-Blaise-sur-Richelieu",
            "Saint-Bonaventure",
            "Saint-Boniface",
            "Saint-Bruno",
            "Saint-Bruno-de-Guigues",
            "Saint-Bruno-de-Kamouraska",
            "Saint-Bruno-de-Montar",
            "Saint-Calixte",
            "Saint-Camille",
            "Saint-Camille-de-Lellis",
            "Saint-Casimir",
            "Saint-Célestin",
            "Saint-Césaire",
            "Saint-Charles-Borromée",
            "Saint-Charles-de-Bellechasse",
            "Saint-Charles-de-Bourget",
            "Saint-Charles-Garnier",
            "Saint-Charles-sur-Richelieu",
            "Saint-Christophe-d'Arthabaska",
            "Saint-Chrysostome",
            "Saint-Claude",
            "Saint-Clément",
            "Saint-Cléophas",
            "Saint-Cléophas-de-Brandon",
            "Saint-Clet",
            "Saint-Colomban",
            "Saint-Côme",
            "Saint-Côme–Linière",
            "Saint-Constant",
            "Saint-Cuthbert",
            "Saint-Cyprien",
            "Saint-Cyprien-de-Napier",
            "Saint-Cyrille-de-Lessard",
            "Saint-Cyrille-de-Wendover",
            "Saint-Damase",
            "Saint-Damase-de-L'Islet",
            "Saint-Damien",
            "Saint-Damien-de-Buckland",
            "Saint-David",
            "Saint-David-de-Falardeau",
            "Saint-Denis-de-Brompton",
            "Saint-Denis-De La Bouteillerie",
            "Saint-Denis-sur-Richelieu",
            "Saint-Didace",
            "Saint-Dominique",
            "Saint-Dominique-du-Rosaire",
            "Saint-Donat",
            "Sainte-Adèle",
            "Sainte-Agathe-de-Lotbinière",
            "Sainte-Agathe-des-Monts",
            "Sainte-Angèle-de-Mérici",
            "Sainte-Angèle-de-Monnoir",
            "Sainte-Angèle-de-Prémont",
            "Sainte-Anne-de-Beaupré",
            "Sainte-Anne-de-Bellevue",
            "Sainte-Anne-de-la-Pérade",
            "Sainte-Anne-de-la-Pocatière",
            "Sainte-Anne-de-la-Rochelle",
            "Sainte-Anne-de-Sabrevois",
            "Sainte-Anne-des-Lacs",
            "Sainte-Anne-des-Monts",
            "Sainte-Anne-de-Sorel",
            "Sainte-Anne-des-Plaines",
            "Sainte-Anne-du-Lac",
            "Sainte-Apolline-de-Patton",
            "Sainte-Aurélie",
            "Sainte-Barbe",
            "Sainte-Béatrix",
            "Sainte-Brigide-d'Iber",
            "Sainte-Brigitte-de-Laval",
            "Sainte-Brigitte-des-Saults",
            "Sainte-Catherine",
            "Sainte-Catherine-de-Hatley",
            "Sainte-Catherine-de-la-Jacques-Cartier",
            "Sainte-Cécile-de-Lévrard",
            "Sainte-Cécile-de-Milton",
            "Sainte-Cécile-de-Whitton",
            "Sainte-Christine",
            "Sainte-Christine-d'Auvergne",
            "Sainte-Claire",
            "Sainte-Clotilde",
            "Sainte-Clotilde-de-Beauce",
            "Sainte-Clotilde-de-Horton",
            "Sainte-Croix",
            "Saint-Edmond-de-Grantham",
            "Saint-Edmond-les-Plaines",
            "Saint-Édouard",
            "Saint-Édouard-de-Fabre",
            "Saint-Édouard-de-Lotbinière",
            "Saint-Édouard-de-Maskinongé",
            "Sainte-Edwidge-de-Clifton",
            "Sainte-Élisabeth",
            "Sainte-Élizabeth-de-Warwick",
            "Sainte-Émélie-de-l'Énergie",
            "Sainte-Eulalie",
            "Sainte-Euphémie-sur-Rivière-du-Sud",
            "Sainte-Famille-de-l'Île-d'Orléans",
            "Sainte-Félicité",
            "Sainte-Flavie",
            "Sainte-Florence",
            "Sainte-Françoise",
            "Sainte-Geneviève-de-Batiscan",
            "Sainte-Geneviève-de-Berthier",
            "Sainte-Germaine-Boulé",
            "Sainte-Gertrude-Manne",
            "Sainte-Hedwidge",
            "Sainte-Hélène-de-Bagot",
            "Sainte-Hélène-de-Chester",
            "Sainte-Hélène-de-Kamouraska",
            "Sainte-Hélène-de-Mancebourg",
            "Sainte-Hénédine",
            "Sainte-Irène",
            "Sainte-Jeanne-d'Arc",
            "Sainte-Julie",
            "Sainte-Julienne",
            "Sainte-Justine",
            "Sainte-Justine-de-Newton",
            "Saint-Élie-de-Caxton",
            "Saint-Éloi",
            "Sainte-Louise",
            "Saint-Elphège",
            "Sainte-Luce",
            "Sainte-Lucie-de-Beauregard",
            "Sainte-Lucie-des-Laurentides",
            "Saint-Elzéar",
            "Saint-Elzéar-de-Témiscouata",
            "Sainte-Madeleine",
            "Sainte-Madeleine-de-la-Rivière-Madeleine",
            "Sainte-Marcelline-de-Kildare",
            "Sainte-Marguerite",
            "Sainte-Marguerite-du-Lac-Masson",
            "Sainte-Marguerite-Marie",
            "Sainte-Marie",
            "Sainte-Marie-de-Blandford",
            "Sainte-Marie-Madeleine",
            "Sainte-Marie-Salomé",
            "Sainte-Marthe",
            "Sainte-Marthe-sur-le-Lac",
            "Sainte-Martine",
            "Sainte-Mélanie",
            "Saint-Émile-de-Suffolk",
            "Sainte-Monique",
            "Sainte-Paule",
            "Sainte-Perpétue",
            "Sainte-Pétronille",
            "Saint-Éphrem-de-Beauce",
            "Saint-Épiphane",
            "Sainte-Praxède",
            "Sainte-Rita",
            "Sainte-Rose-de-Watford",
            "Sainte-Rose-du-Nord",
            "Sainte-Sabine",
            "Sainte-Séraphine",
            "Sainte-Sophie",
            "Sainte-Sophie-de-Lévrard",
            "Sainte-Sophie-d'Halifax",
            "Saint-Esprit",
            "Sainte-Thècle",
            "Sainte-Thérèse",
            "Sainte-Thérèse-de-Gaspé",
            "Sainte-Thérèse-de-la-Gatineau",
            "Saint-Étienne-de-Beauharnois",
            "Saint-Étienne-de-Bolton",
            "Saint-Étienne-des-Grès",
            "Saint-Eugène",
            "Saint-Eugène-d'Argentenay",
            "Saint-Eugène-de-Guigues",
            "Saint-Eugène-de-Ladrière",
            "Sainte-Ursule",
            "Saint-Eusèbe",
            "Saint-Eustache",
            "Saint-Évariste-de-Forsyth",
            "Sainte-Victoire-de-Sorel",
            "Saint-Fabien",
            "Saint-Fabien-de-Panet",
            "Saint-Faustin–Lac-Carré",
            "Saint-Félicien",
            "Saint-Félix-de-Dalquier",
            "Saint-Félix-de-Kingsey",
            "Saint-Félix-de-Valois",
            "Saint-Félix-d'Otis",
            "Saint-Ferdinand",
            "Saint-Ferréol-les-Neiges",
            "Saint-Flavien",
            "Saint-Fortunat",
            "Saint-François-d'Assise",
            "Saint-François-de-la-Rivière-du-Sud",
            "Saint-François-de-l'Île-d'Orléans",
            "Saint-François-de-Sales",
            "Saint-François-du-Lac",
            "Saint-François-Xavier-de-Brompton",
            "Saint-François-Xavier-de-Viger",
            "Saint-Frédéric",
            "Saint-Fulgence",
            "Saint-Gabriel",
            "Saint-Gabriel-de-Brandon",
            "Saint-Gabriel-de-Rimouski",
            "Saint-Gabriel-de-Valcartier",
            "Saint-Gabriel-Lalemant",
            "Saint-Gédéon",
            "Saint-Gédéon-de-Beauce",
            "Saint-Georges",
            "Saint-Georges-de-Clarence",
            "Saint-Georges-de-Windsor",
            "Saint-Gérard-Majella",
            "Saint-Germain",
            "Saint-Germain-de-Grantham",
            "Saint-Gervais",
            "Saint-Gilbert",
            "Saint-Gilles",
            "Saint-Godefroi",
            "Saint-Guillaume",
            "Saint-Guillaume-Nord",
            "Saint-Guy",
            "Saint-Henri",
            "Saint-Henri-de-Taillon",
            "Saint-Herménégilde",
            "Saint-Hilaire-de-Dorset",
            "Saint-Hilarion",
            "Saint-Hippolyte",
            "Saint-Honoré",
            "Saint-Honoré-de-Shenley",
            "Saint-Honoré-de-Témiscouata",
            "Saint-Hubert-de-Rivière-du-Loup",
            "Saint-Hugues",
            "Saint-Hyacinthe",
            "Saint-Ignace-de-Loyola",
            "Saint-Ignace-de-Stanbridge",
            "Saint-Irénée",
            "Saint-Isidore",
            "Saint-Isidore-de-Clifton",
            "Saint-Jacques",
            "Saint-Jacques-de-Leeds",
            "Saint-Jacques-le-Majeur-de-Wolfestown",
            "Saint-Jacques-le-Mineur",
            "Saint-Janvier-de-Joly",
            "Saint-Jean-Baptiste",
            "Saint-Jean-de-Brébeuf",
            "Saint-Jean-de-Cherbourg",
            "Saint-Jean-de-Dieu",
            "Saint-Jean-de-la-Lande",
            "Saint-Jean-de-l'Île-d'Orléans",
            "Saint-Jean-de-Matha",
            "Saint-Jean-Port-Joli",
            "Saint-Jean-sur-Richelieu",
            "Saint-Jérôme",
            "Saint-Joachim",
            "Saint-Joachim-de-Shefford",
            "Saint-Joseph-de-Beauce",
            "Saint-Joseph-de-Coleraine",
            "Saint-Joseph-de-Kamouraska",
            "Saint-Joseph-de-Lepage",
            "Saint-Joseph-des-Érables",
            "Saint-Joseph-de-Sorel",
            "Saint-Joseph-du-Lac",
            "Saint-Jude",
            "Saint-Jules",
            "Saint-Julien",
            "Saint-Just-de-Bretenières",
            "Saint-Juste-du-Lac",
            "Saint-Justin",
            "Saint-Lambert",
            "Saint-Lambert-de-Lauzon",
            "Saint-Laurent-de-l'Île-d'Orléans",
            "Saint-Lazare",
            "Saint-Lazare-de-Bellechasse",
            "Saint-Léandre",
            "Saint-Léonard-d'Aston",
            "Saint-Léonard-de-Portneuf",
            "Saint-Léon-de-Standon",
            "Saint-Léon-le-Grand",
            "Saint-Liboire",
            "Saint-Liguori",
            "Saint-Lin–Laurentides",
            "Saint-Louis",
            "Saint-Louis-de-Blandford",
            "Saint-Louis-de-Gonzague",
            "Saint-Louis-de-Gonzague-du-Cap-Tourmente",
            "Saint-Louis-du-Ha! Ha!",
            "Saint-Luc-de-Bellechasse",
            "Saint-Luc-de-Vincennes",
            "Saint-Lucien",
            "Saint-Ludger",
            "Saint-Ludger-de-Milot",
            "Saint-Magloire",
            "Saint-Majorique-de-Grantham",
            "Saint-Malachie",
            "Saint-Malo",
            "Saint-Marc-de-Figuery",
            "Saint-Marc-des-Carrières",
            "Saint-Marc-du-Lac-Long",
            "Saint-Marcel",
            "Saint-Marcel-de-Richelieu",
            "Saint-Marcellin",
            "Saint-Marc-sur-Richelieu",
            "Saint-Martin",
            "Saint-Mathias-sur-Richelieu",
            "Saint-Mathieu",
            "Saint-Mathieu-de-Belœil",
            "Saint-Mathieu-de-Rioux",
            "Saint-Mathieu-d'Harricana",
            "Saint-Mathieu-du-Parc",
            "Saint-Maurice",
            "Saint-Maxime-du-Mont-Louis",
            "Saint-Médard",
            "Saint-Michel",
            "Saint-Michel-de-Bellechasse",
            "Saint-Michel-des-Saints",
            "Saint-Michel-du-Squatec",
            "Saint-Modeste",
            "Saint-Moïse",
            "Saint-Narcisse",
            "Saint-Narcisse-de-Beaurivage",
            "Saint-Narcisse-de-Rimouski",
            "Saint-Nazaire",
            "Saint-Nazaire-d'Acton",
            "Saint-Nazaire-de-Dorchester",
            "Saint-Nérée-de-Bellechasse",
            "Saint-Noël",
            "Saint-Norbert",
            "Saint-Norbert-d'Arthabaska",
            "Saint-Octave-de-Métis",
            "Saint-Odilon-de-Cranbourne",
            "Saint-Omer",
            "Saint-Onésime-d'Ixworth",
            "Saint-Ours",
            "Saint-Pacôme",
            "Saint-Pamphile",
            "Saint-Pascal",
            "Saint-Patrice-de-Beaurivage",
            "Saint-Patrice-de-Sherrington",
            "Saint-Paul",
            "Saint-Paul-d'Abbotsford",
            "Saint-Paul-de-la-Croix",
            "Saint-Paul-de-l'Île-aux-Noix",
            "Saint-Paul-de-Montminy",
            "Saint-Paulin",
            "Saint-Philémon",
            "Saint-Philibert",
            "Saint-Philippe",
            "Saint-Philippe-de-Néri",
            "Saint-Pie",
            "Saint-Pie-de-Guire",
            "Saint-Pierre",
            "Saint-Pierre-Baptiste",
            "Saint-Pierre-de-Broughton",
            "Saint-Pierre-de-Lamy",
            "Saint-Pierre-de-la-Rivière-du-Sud",
            "Saint-Pierre-de-l'Île-d'Orléans",
            "Saint-Pierre-les-Becquets",
            "Saint-Placide",
            "Saint-Polycarpe",
            "Saint-Prime",
            "Saint-Prosper",
            "Saint-Prosper-de-Champlain",
            "Saint-Raphaël",
            "Saint-Raymond",
            "Saint-Rémi",
            "Saint-Rémi-de-Tingwick",
            "Saint-René",
            "Saint-René-de-Matane",
            "Saint-Robert",
            "Saint-Robert-Bellarmin",
            "Saint-Roch-de-l'Achigan",
            "Saint-Roch-de-Mékinac",
            "Saint-Roch-de-Richelieu",
            "Saint-Roch-des-Aulnaies",
            "Saint-Roch-Ouest",
            "Saint-Romain",
            "Saint-Rosaire",
            "Saint-Samuel",
            "Saints-Anges",
            "Saint-Sauveur",
            "Saint-Sébastien",
            "Saint-Sévère",
            "Saint-Séverin",
            "Saint-Siméon",
            "Saint-Simon",
            "Saint-Simon-de-Rimouski",
            "Saint-Simon-les-Mines",
            "Saint-Sixte",
            "Saints-Martyrs-Canadiens",
            "Saint-Stanislas",
            "Saint-Stanislas-de-Kostka",
            "Saint-Sulpice",
            "Saint-Sylvère",
            "Saint-Sylvestre",
            "Saint-Télesphore",
            "Saint-Tharcisius",
            "Saint-Théodore-d'Acton",
            "Saint-Théophile",
            "Saint-Thomas",
            "Saint-Thomas-Didyme",
            "Saint-Thuribe",
            "Saint-Tite",
            "Saint-Tite-des-Caps",
            "Saint-Ubalde",
            "Saint-Ulric",
            "Saint-Urbain",
            "Saint-Urbain-Premier",
            "Saint-Valentin",
            "Saint-Valère",
            "Saint-Valérien",
            "Saint-Valérien-de-Milton",
            "Saint-Vallier",
            "Saint-Venant-de-Paquette",
            "Saint-Vianney",
            "Saint-Victor",
            "Saint-Wenceslas",
            "Saint-Zacharie",
            "Saint-Zénon",
            "Saint-Zénon-du-Lac-Humqui",
            "Saint-Zéphirin-de-Courval",
            "Saint-Zotique",
            "Salaberry-de-Valleyfield",
            "Salluit",
            "Sault-au-Cochon",
            "Sayabec",
            "Scheffer",
            "Scotstown",
            "Scott",
            "Senneterre",
            "Senne",
            "Sept-Îles",
            "Shannon",
            "Shawinigan",
            "Shaw",
            "Sheenboro",
            "Shefford",
            "Sherbrooke",
            "Shigawake",
            "Sorel-Tracy",
            "Stanbridge East",
            "Stanbridge Station",
            "Stanstead",
            "Stanstead-Est",
            "Stoke",
            "Stoneham-et-Tewkesbury",
            "Stornoway",
            "Stratford",
            "Stukely-Sud",
            "Sutton",
            "Tadoussac",
            "Taschereau",
            "Tasiujaq",
            "Témiscaming",
            "Témiscouata-sur-le-Lac",
            "Terrasse-Vaudreuil",
            "Terrebonne",
            "Thetford Mines",
            "Thorne",
            "Thurso",
            "Timiskaming",
            "Tingwick",
            "Tour",
            "Trécesson",
            "Très-Saint-Rédempteur",
            "Très-Saint-Sacrement",
            "Tring-Jonction",
            "Trois-Pistoles",
            "Trois-Rives",
            "Trois-Rivières",
            "Uashat",
            "Ulverton",
            "Umiujaq",
            "Upton",
            "Val-Alain",
            "Val-Brillant",
            "Valcourt",
            "Val-David",
            "Val-des-Bois",
            "Val-des-Lacs",
            "Val-des-Monts",
            "Val-des-Sources",
            "Val-d'Or",
            "Val-Joli",
            "Vallée-Jonction",
            "Val-Morin",
            "Val-Racine",
            "Val-Saint-Gilles",
            "Varennes",
            "Vaudreuil-Dorion",
            "Vaudreuil-sur-le-Lac",
            "Venise-en-Québec",
            "Verchères",
            "Victoria",
            "Ville-Marie",
            "Villeroy",
            "Waltham",
            "Warden",
            "Warwick",
            "Waskaganish",
            "Waswanipi",
            "Waterloo",
            "Water",
            "Weedon",
            "Wemindji",
            "Wemotaci",
            "Wendake",
            "Wentworth",
            "Wentworth-Nord",
            "Westbury",
            "Westmount",
            "Whapmagoostui",
            "Wickham",
            "Windsor",
            "Winneway",
            "Wôlinak",
            "Wotton",
            "Yamachiche",
            "Yamaska",
            };

        #endregion
    }
}
