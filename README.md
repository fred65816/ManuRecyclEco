# ManuRecyclEco

## Présentation

ManuRecyclEco est un prototype de plateforme d'échange et de vente de manuels scolaires usagés crée par une équipe de trois étudiants de l'UQAM  dans le cadre du cours [INM5151](https://etudier.uqam.ca/cours?sigle=INM5151) du Baccalauréat en informatique et génie logiciel. Le prototype a été codé en WPF/C# et il utilise le Entity Framework pour l'interaction avec la base de données SQLite. 

Bien que cela fut un projet d'équipe j'ai codé environ 65% à 75% de l'application seul. Le prototype a été realisé en 3 sprints de 3 semaines chacun. Le code du dépôt montre l'avancement à la fin du 3e sprint. Le prototype comprend une page d'authetification, de création de compte, de réinitialisation de mot de passe, de profil, de recherche, de livre disponible pour les cours de l'étudiant, de publication d'offres et de modification des offres de l'usager. Ces pages seront décrites plus en détail dans les sections ci-dessous.

## Base de données

Une base de données test peut être crée avec le fichier `ApplicationDbContext.cs`. Elle comprends des usagers, des livres, des exemplaires, des cours, des programmes académiques. Les images de livres sont tirées du dossier `livres`. Il y a possibilité d'utiliser seulement certain livres aléatoirement ou bien le maximum de 93. De plus on peut générer un nombre au choix d'exemplaires pour ces livres et avoir des caractéristiques aléatoire pour ceux-cis tel le prix, l'état et le type de transaction recherché.

Voici le diagramme de classes utilisé par le Entity Framework pour la base de données:

![model](images/model.png)

## Authentification

![login](images/login.png)

La section d'authetification comprend la page d'authentification, une page de création de compte, une page de réinitialisation de mot de passe et une page de confirmation de courriel. Cette dernière est utilisée autant par la création de compte que la réinitialisation de mot de passe puisqu'un jeton aléatoire de 16 caractère est envoyé par courriel et l'utilisateur doit entrer le jeton sur cette page. Ce mécanisme est aussi utilisé par le changement d'adresse courriel dans la page de profil utilisateur.

## Profil de l'utilisateur

![profile](images/profile.png)

Le seul champ non-modifiable du profil est le nom d'utilisateur. Outre les champs d'information tel l'adresse courriel, le programme inscrit, le prénom, le nom et la ville, il est aussi possible d'ajouter des cours et des livres recherchés. Les cours peuvent être filtrés par programmes et servent principalement comme filtre dans la page d'exemplaires pour les cours de l'utilisateur. Les livres recherchés peuvent être filtrés par programme et par cours et auraient pu être utilisés dans le cadre d'une notification quand un exemplaire du livre est publié sur l'application mais cela n'a pas été fait puisque la messagerie n'a pas été implémentée. Finalement il est aussi possible de téléverser une image de profil et choisir un style de couleurs pour l'application en entier.

## Exemplaires de cours

![mycourses](images/mycourses.png)

La page d'exemplaires pour les cours de l'utilisateur comprend les offres d'exemplaires des cours ajoutés dans le profil. Il est possible de filtrer les résultats par cours ou bien par livre. Il est possible de consulter le détail d'une offre en cliquant sur "Détail" dans le coin inférieur droit de l'offre.

## Recherche d'exemplaires

![search](images/search.png)

La page de recherche est semblable à la page d'exemplaires pour les cours de l'utilisateur sauf qu'elle regroupe par défault tous les exemplaires de la base de données. Il est possible de filtrer les résultats par programme, cours, livre, titre, auteur, condition, éditeur, type de transaction recherchée, utilisateur, prix de vente, prix de référence, nombre de pages et année de publication. Les résultats sont mis à jour en temps réel, c'est-à-dire à chaque fois que l'utilisateur tape une lettre, un chiffre ou change la sélection d'une liste déroulante. Il y a finalement un bouton pour réinitialiser tous les champs de recherche. Il est possible de consulter le détail d'une offre en cliquant sur "Détail" dans le coin inférieur droit de l'offre.

## Publication d'offre

![publish](images/publish.png)

La publication d'une offre se fait en sélectionnant un livre. Les livres peuvent être filtrés par programme et/ou par cours. Une fois le livre sélectionné, les champs comprenant l'auteur, l'éditeur, l'année, le nombre de pages et l'ISBN sont mis à jour et sont non-modifiables, à moins qu'un autre livre soit sélectionné. L'utilisateur doit entrer le type de transaction (vente, échange, vente ou échange), l'état de l'exemplaire (1 pour mauvais et 10 pour excellent) et un prix dans le cas d'une offre de vente. L'utilisateur peut aussi téléverser une image de l'exemplaire, mais cela est optionel et s'il n'y a pas d'image téléversée l'image de référence du livre sera utilisée dans la page de recherche par exemple.

## Modification d'offres

![mybooks](images/mybooks.png)

La page de modification d'offre comprend la liste des offres publiées par l'utilisateur. Il est possible de modifier le type de transaction, l'état de l'exemplaire et le prix demandé dans la cas d'une offre de vente. Il est aussi possible de modifier l'image de l'offre en téléversant une image de l'exemplaire. Il est finalement possible de supprimer une offre.

## Relation entre les ViewModels du sprint 3

En complément, un diagramme des relations entre les ViewModels utilisés au sprint 3. Seulement les attributs important sont dans ce diagramme et les ViewModels de la section authentification sont manquants puisqu'il ont été complétés au sprint 1.

![viewmodels](images/viewmodels.png)

