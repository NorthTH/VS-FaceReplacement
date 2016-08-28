% ictdoc.cls
%
%
%author: Kanrawi Kitkhachonkunlaphat, Faculty of ICT, Mahidol University
%
%modified from muthesis.cls
%by  Michael A. Allen, Physics Department, Mahidol University
%
% adapted from: /usr/share/texmf/tex/latex/base/report.cls and Thai Latex
% also /usr/share/texmf/tex/latex/natbib/natbib.sty

\NeedsTeXFormat{LaTeX2e}

% modified here
\ProvidesClass{ictdoc}

\LoadClass[11pt,a4paper]{report}

\RequirePackage{fancyheadings}
\RequirePackage[tableposition=top]{caption}
\RequirePackage{ifthen}
\newboolean{@natbibmode}\setboolean{@natbibmode}{false}
\DeclareOption{natbib}{%
  \setboolean{@natbibmode}{true}%
}
\ProcessOptions\relax
\if@natbibmode
  \RequirePackage{natbib}% this cannot go inside \DeclareOption argument
  \bibpunct{[}{]}{;}{a}{}{,}%
\else
\fi

\textwidth 147.5mm
\textheight 234.5mm
\oddsidemargin 12mm
\evensidemargin 12mm
\topmargin 0mm
\newlength{\refbaselineskip}
\setlength{\refbaselineskip}{\baselineskip}
\newcommand{\linespacing}[1]{\baselineskip=#1\refbaselineskip}

% following taken from textcase.sty with extra `%' added to prevent
% extraneous space at start of output from \MakeTextUppercase
\def\@uclcnotmath#1#2#3#4{\begingroup
      #1%
      \def\({$}\let\)\(%$
      \def\NoCaseChange##1{\noexpand\NoCaseChange{\noexpand##1}}%
      \@nonchangecase\label%
      \@nonchangecase\ref%
      \@nonchangecase\ensuremath%
      \def\cite##1##{\toks@{\noexpand\cite##1}\@citex}%
      \def\@citex##1{\NoCaseChange{\the\toks@{##1}}}%
      \def\reserved@a##1##2{\let#2\reserved@a}%
      \expandafter\reserved@a\@uclclist\reserved@b{\reserved@b\@gobble}%
      \protected@edef\reserved@a{\endgroup%
          \noexpand\@skipmath#3#4$\valign$}%
      \reserved@a}%
\def\@nonchangecase#1{\def#1##1{\NoCaseChange{#1{##1}}}}%
\let\NoCaseChange\@firstofone%
\def\@skipmath#1#2$#3${%
  \@skip@nonchangecase#1#2\NoCaseChange\valign%
  \ifx\valign#3%
  \else%
    $#3$%
    \expandafter\@skipmath\expandafter#1%
  \fi}%
\def\@skip@nonchangecase#1#2\NoCaseChange#3{%
  #1{#2}%
  \ifx\valign#3%
  \else
    #3%
    \expandafter\@skip@nonchangecase\expandafter#1%
  \fi}
\DeclareRobustCommand\MakeTextUppercase{%
\@uclcnotmath{\def\i{I}\def\j{J}}{##1##2}\uppercase}
\protected@edef\MakeTextUppercase#1{\MakeTextUppercase{#1}}

% from thai style file

\def\tb{\hspace{0cm}} % Word Break Characters
%\def\tb{\linebreak[3]\hspace{0cm}} % Word Break Characters
\def\thai#1{\utfont #1} % Switch to Thai Font command
\def\spsizenormal{18} % for 11pt
%\def\spsizenormal{21.6} % for 12pt
\def\tfontname{dbtt}
\def\tfontsize{\spsizenormal pt}
\def\huispace{ }
\def\settfont{\font\utfont = \tfontname\huispace  at \tfontsize}
\settfont % initialize thai font and size

\renewcommand\bibname{References}

% draft mode

\newboolean{@draftmode}\setboolean{@draftmode}{false}
\newcommand{\draft}{\setboolean{@draftmode}{true}}
\newboolean{@notitlepage}\setboolean{@notitlepage}{false}
\newcommand{\notitlepage}{\setboolean{@notitlepage}{true}}


%%%%%%%%%%%%%VARIABLES AND INFORMATION SETTING%%%%%%%%%%%%%%%

% information for front page

\newcommand{\candidatename}[1]{\def\@candidatename{#1}}
\newcommand{\candidatenametwo}[1]{\def\@candidatenametwo{#1}}
\newcommand{\candidatenamethree}[1]{\def\@candidatenamethree{#1}}

\newcommand{\candidatesurname}[1]{\def\@candidatesurname{#1}}
\newcommand{\candidatesurnametwo}[1]{\def\@candidatesurnametwo{#1}}
\newcommand{\candidatesurnamethree}[1]{\def\@candidatesurnamethree{#1}}

\newcommand{\candidateshort}[1]{\def\@candidateshort{#1}}
\newcommand{\candidateshorttwo}[1]{\def\@candidateshorttwo{#1}}
\newcommand{\candidateshortthree}[1]{\def\@candidateshortthree{#1}}

\newcommand{\degree}[1]{\def\@degree{#1} \def\@fulldegree{Bachelor of Science}}

\newcommand{\subject}[1]{\def\@subject{#1}}
\newcommand{\subjectshort}[1]{\def\@subjectshort{#1}}
\newcommand{\submissionyear}[1]{\def\@submissionyear{#1}}
\newcommand{\@TITLE}{\MakeTextUppercase{\@title}}


\newcommand{\candidatetitle}[1]{\def\@candidatetitle{#1}}
\newcommand{\candidatetitletwo}[1]{\def\@candidatetitletwo{#1}}
\newcommand{\candidatetitlethree}[1]{\def\@candidatetitlethree{#1}}

\newcommand{\majoradvisor}[1]{\def\@majoradvisor{#1}}


% information for page iii (ABSTRACT)

\newcommand{\candidatenumber}[1]{\def\@candidatenumber{#1}}
\newcommand{\candidatenumbertwo}[1]{\def\@candidatenumbertwo{#1}}
\newcommand{\candidatenumberthree}[1]{\def\@candidatenumberthree{#1}}

\newcommand{\candidatefaccode}[1]{\def\@candidatefaccode{#1}}

\newcommand{\major}[1]{\def\@major{#1}}
\newboolean{@longsubject}\setboolean{@longsubject}{false}
\newcommand{\longsubject}{\setboolean{@longsubject}{true}}
\newcommand{\keywords}[1]{\def\@keywords{#1}}
\newcommand{\keywordsII}[1]{\def\@keywordsII{#1}}
\newcommand{\keywordsIII}[1]{\def\@keywordsIII{#1}}

% information for page iv (THAI ABSTRACT)

\newcommand{\thaimajor}[1]{\def\@thaimajor{#1}}
\newcommand{\thaisubject}[1]{\def\@thaisubject{#1}}

\newcommand{\thaicandidatefullname}[1]{\def\@thaicandidatefullname{#1}}
\newcommand{\thaicandidatefullnametwo}[1]{\def\@thaicandidatefullnametwo{#1}}
\newcommand{\thaicandidatefullnamethree}[1]{\def\@thaicandidatefullnamethree{#1}}

\newcommand{\thaititle}[1]{\def\@thaititle{#1}}
\newcommand{\thaimajoradvisor}[1]{\def\@thaimajoradvisor{#1}}


% information for Biography

\newcommand{\dateofbirth}[1]{\def\@dateofbirth{#1}}
\newcommand{\placeofbirth}[1]{\def\@placeofbirth{#1}}
\newcommand{\firstdegree}[1]{\def\@firstdegree{#1}}
\newboolean{@longfirstdegree}\setboolean{@longfirstdegree}{false}
\newcommand{\longfirstdegree}{\setboolean{@longfirstdegree}{true}}
\newcommand{\firstdegreeinstitution}[1]{\def\@firstdegreeinstitution{#1}}
\newcommand{\firstdegreeyears}[1]{\def\@firstdegreeyears{#1}}
\newcommand{\years}[1]{\def\@years{#1}}


\newcommand{\dateofbirthtwo}[1]{\def\@dateofbirthtwo{#1}}
\newcommand{\placeofbirthtwo}[1]{\def\@placeofbirthtwo{#1}}
\newcommand{\firstdegreetwo}[1]{\def\@firstdegreetwo{#1}}
\newboolean{@longfirstdegreetwo}\setboolean{@longfirstdegreetwo}{false}
\newcommand{\longfirstdegreetwo}{\setboolean{@longfirstdegreetwo}{true}}
\newcommand{\firstdegreeinstitutiontwo}[1]{\def\@firstdegreeinstitutiontwo{#1}}
\newcommand{\firstdegreeyearstwo}[1]{\def\@firstdegreeyearstwo{#1}}
\newcommand{\yearstwo}[1]{\def\@yearstwo{#1}}


\newcommand{\dateofbirththree}[1]{\def\@dateofbirththree{#1}}
\newcommand{\placeofbirththree}[1]{\def\@placeofbirththree{#1}}
\newcommand{\firstdegreethree}[1]{\def\@firstdegreethree{#1}}
\newboolean{@longfirstdegreethree}\setboolean{@longfirstdegreethree}{false}
\newcommand{\longfirstdegreethree}{\setboolean{@longfirstdegreethree}{true}}
\newcommand{\firstdegreeinstitutionthree}[1]{\def\@firstdegreeinstitutionthree{#1}}
\newcommand{\firstdegreeyearsthree}[1]{\def\@firstdegreeyearsthree{#1}}
\newcommand{\yearsthree}[1]{\def\@yearsthree{#1}}
%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%

% alternative heading used for CONTENTS (Cont.), etc
\newcommand{\@althead}{}
% right-hand heading used for Appendix section
\newcommand{\@appname}{}

\def\@facundergradstud{ Faculty of ICT, Mahidol Univ.}

\def\@sigdots{..........................................}
% signature dots followed by parbox
\newcommand{\@sbx}[1]{\parbox[t]{70mm}{\RaggedRight\@sigdots\\#1}}
\newlength{\@halftextwidth}
\@halftextwidth=0.53\textwidth
\def\@thesisentitled{\vspace*{-15mm}Thesis \\ entitled \\ \vskip 1em
	\large \bfseries \@TITLE \\
	\normalsize\rm
}
\author{\@candidate}
\date{}

\renewcommand\maketitle{\par
	\linespacing{1.3}
	\begingroup
	\renewcommand\thefootnote{\@fnsymbol\c@footnote}
    	\def\@makefnmark{\rlap{\@textsuperscript{\normalfont\@thefnmark}}}
    	\long\def\@makefntext##1{\parindent 1em\noindent
		\hb@xt@1.8em{%
                	\hss\@textsuperscript{\normalfont\@thefnmark}}##1}
	\if@twocolumn
	\ifnum \col@number=\@ne
        \@maketitle
      	\else
        \twocolumn[\@maketitle]%
	\fi
	\else
	\newpage
	\global\@topnum\z@   % Prevents figures from going at top of page.
      	\@maketitle
    	\fi
    	\thispagestyle{empty}\@thanks
  	\endgroup
	\setcounter{footnote}{0}%
	\global\let\thanks\relax
	\global\let\maketitle\relax
	\global\let\@maketitle\relax
	\global\let\@thanks\@empty
	\global\let\@author\@empty
	\global\let\@date\@empty
%	\global\let\@title\@empty
	\global\let\title\relax
    \global\let\titleth\relax
	\global\let\author\relax
	\global\let\date\relax
	\global\let\and\relax
}


%%%%%%%%%%%%%%%%%%%%%%FIRST PAGE%%%%%%%%%%%%%%%%%%%%%%

\def\@maketitle{
\if@draftmode\else\setboolean{@notitlepage}{false}\fi
\if@notitlepage\else
	\newpage
	\thispagestyle{empty}	
    
	 \begin{center}
	\Large \bfseries\@TITLE \par 
     {\vskip 0.5em}
	\Large \bfseries \@thaititle \par
  	\null
\if@draftmode\@author\vskip 2em\else
  	\let \footnote \thanks
    	\vskip 5.0em
    	{\Large
      	\lineskip .5em
        \begin{center}
        {BY}  \vspace{-7mm}\\
        \end{center}         

        \begin{tabular}{l l l l}

         \MakeUppercase\@candidatetitle&\MakeUppercase\@candidatename~~&\MakeUppercase\@candidatesurname&\@candidatenumber\vspace{-4mm}\\
         \MakeUppercase\@candidatetitletwo&\MakeUppercase\@candidatenametwo~~&\MakeUppercase\@candidatesurnametwo&\@candidatenumbertwo\vspace{-4mm}\\
         \MakeUppercase\@candidatetitlethree&\MakeUppercase\@candidatenamethree~~&\MakeUppercase\@candidatesurnamethree&\@candidatenumberthree\\

        \end{tabular}

        {\vskip 4.5em}
        \begin{center}
        {ADVISOR} \\
        {\MakeUppercase\@majoradvisor} \\
        \end{center}\par}
    	\vskip 6.0em

    {\large A Senior Project Submitted in Partial Fulfillment of \\
    the Requirements for \\ 
    \vskip 1em
    THE DEGREE OF BACHELOR OF SCIENCE  \\
	(INFORMATION AND COMMUNICATION TECHNOLOGY) \\
    \vskip 1em
	Faculty of Information and Communication Technology \\
	Mahidol University \\
	\@submissionyear \\ \mbox{}\\
    }
  	\end{center}
  	\par
  	\vskip 1.5em
}
%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%


%%%%%%%%%%%%%%%%%ACKNOWLEDGEMENT%%%%%%%%%%%%%%%%%%%%%
\newcommand{\acknowledgement}[1]{\if@draftmode\else
        \chapter*{Acknowledgement}
	\thispagestyle{empty}
  	\markright{Senior Project}
	\addcontentsline{toc}{chapter}{ACKNOWLEDGEMENT}
	\linespacing{1.3}
        #1\mbox{}\newline\newline\mbox{}\hfill\@candidatetitle\@candidatename~~\@candidatesurname\\
        \mbox{}\hfill\@candidatetitletwo\@candidatenametwo~~\@candidatesurnametwo\\
        \mbox{}\hfill\@candidatetitlethree\@candidatenamethree ~~\@candidatesurnamethree\\
\fi
}

\def\@abstractbox{
	\thispagestyle{myheadings}
	\markright{
	\hspace*{-0.05\textwidth}
	\protect\raisebox{-9mm}{
%	\fbox{\protect\rule[-\textheight]{0pt}{0pt}\hspace*{1.04\textwidth}}}
%	\hspace*{-1.03\textwidth}\small\@facundergradstud\hfil Senior Project / \thepage
	\fbox{\protect\rule[-\textheight]{0pt}{0pt}\hspace*{1.03\textwidth}}}
	\hspace*{-1.03\textwidth}\small\@facundergradstud\hfil Senior Project / \thepage
	}
}
%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%


%%%%%%%%%%%%%%%%%%%%ABSTRACT (ENGLISH)%%%%%%%%%%%%%%%%%%

\renewcommand{\abstract}[1]{\if@draftmode\if@notitlepage\else
 #1 \newpage \fi
\else
	\newpage
	\addcontentsline{toc}{chapter}{ABSTRACT (ENGLISH)}
	\@abstractbox

    \begin{flushleft}
    \begin{tabular}{l l}
    \multicolumn{2}{l}{\@TITLE} \vspace{2mm}\\
    \MakeUppercase\@candidatename~~\MakeUppercase\@candidatesurname~~&\@candidatenumber~~\@candidatefaccode\vspace{-4mm} \\
    \MakeUppercase\@candidatenametwo~~\MakeUppercase\@candidatesurnametwo~~&\@candidatenumbertwo~~\@candidatefaccode\vspace{-4mm}\\
    \MakeUppercase\@candidatenamethree~~\MakeUppercase\@candidatesurnamethree~~&\@candidatenumberthree~~\@candidatefaccode\vspace{2mm}\\
    \multicolumn{2}{l}{\@degree\ (\MakeUppercase\@subject)}\vspace{3mm}\\  
    \multicolumn{2}{l}{PROJECT ADVISOR : \MakeUppercase\@majoradvisor} \vspace{2mm}\\
    \end{tabular}
    \end{flushleft}

	\begin{center}ABSTRACT\end{center}
	\@beginparpenalty\@lowpenalty
        \@endparpenalty\@M
	\par #1
        \begin{tabbing}
	KEY WORDS~: \= \MakeTextUppercase\@keywords \\
	\ifthenelse{\equal{\@keywordsII}{}}{}{
	 \> \MakeTextUppercase\@keywordsII\\ }
	\ifthenelse{\equal{\@keywordsIII}{}}{}{
	 \> \MakeTextUppercase\@keywordsIII\\ } \\ \vspace{-5mm}%
	\pageref{biography}\ P\@.
      	\end{tabbing}\fussy
\fi
}
%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%


%%%%%%%%%%%%%%%%%%%%%ABSTRACT (ไทย)%%%%%%%%%%%%%%%%%%%%

\newcommand{\thaiabstract}[1]{\if@draftmode\else
	%\titlepage
	\linespacing{1.3}
	\newpage
	\addcontentsline{toc}{chapter}{ABSTRACT (ไทย)}
	\@abstractbox

    \begin{flushleft}
    \begin{tabular}{l l}
    \multicolumn{2}{l}{\@thaititle} \vspace{2mm}\\
    \MakeUppercase\@thaicandidatefullname~~&\@candidatenumber~~\@candidatefaccode\vspace{-4mm} \\
    \MakeUppercase\@thaicandidatefullnametwo~~&\@candidatenumbertwo~~\@candidatefaccode\vspace{-4mm}\\
    \MakeUppercase\@thaicandidatefullnamethree~~&\@candidatenumberthree~~\@candidatefaccode\vspace{2mm}\\
    \multicolumn{2}{l}{\thai วท.บ. (เทคโนโลยีสารสนเทศและการสื่อสาร)}\vspace{3mm}\\  
    \multicolumn{2}{l}{{\thai อาจารย์ที่ปรึกษาโครงการ}  : \@thaimajoradvisor}\vspace{2mm}\\ 
    \end{tabular}
    \end{flushleft}

	\begin{center}{\thai บทคัดย่อ}\end{center}
	\par
	\linespacing{1.5}
	\sloppy #1 \par
        \fussy\mbox{}\newline
   \begin{tabbing}
	\pageref{biography} {\thai หน้า}
      	\end{tabbing}\fussy
\fi
}

\renewcommand\chapter{\if@openright\cleardoublepage\else\clearpage\fi
%	\thispagestyle{plain}
	\global\@topnum\z@
	\@afterindentfalse
	\secdef\@chapter\@schapter
}
% used to make optional argument the heading only (not the toc entry)
\def\@chapter[#1]#2{\ifnum \c@secnumdepth >\m@ne
                         \refstepcounter{chapter}%
                         \typeout{\@chapapp\space\thechapter.}%
                         \addcontentsline{toc}{chapter}%
                                   {\protect\numberline{\thechapter}#2}%(was 1)
                    \else
                      \addcontentsline{toc}{chapter}{#2}% (was 1)
                    \fi
                    \chaptermark{#1}%
                    \addtocontents{lof}{\protect\addvspace{10\p@}}%
                    \addtocontents{lot}{\protect\addvspace{10\p@}}%
                    \if@twocolumn
                      \@topnewpage[\@makechapterhead{#2}]%
                    \else
                      \@makechapterhead{#2}%
                      \@afterheading
                    \fi}
\def\@makechapterhead#1{
	\ifthenelse{\equal{\thechapter }{1}}{\renewcommand{\@althead}{}
		\pagestyle{headings}
		\pagenumbering{arabic}}{}
        \ifthenelse{\equal{\thechapter}{A}}{\renewcommand{\@althead}{}}{}
	\vspace*{5\p@}
	{\parindent \z@ \raggedright \normalfont
	\ifnum \c@secnumdepth >\m@ne
	\begin{center}
        \Large\bfseries \MakeTextUppercase\@chapapp\thechapter
%        \large\bfseries \MakeTextUppercase\@chapapp\space \@arabic\c@chapter
	\end{center}
        \par\nobreak
        \vskip 10\p@
	\fi
	\interlinepenalty\@M
	\begin{center}
    \vspace{-7mm}
	\Large \bfseries\MakeTextUppercase{#1}\par\nobreak
	\end{center}
	\vskip 20\p@
}}

\def\@schapter#1{
	\if@twocolumn
	\@topnewpage[\@makeschapterhead{#1}]
	\else
	\@makeschapterhead{#1}%
	\@afterheading
	\fi
	%\@doublespaced
}
\def\@makeschapterhead#1{
	\vspace*{5\p@}
	{\parindent \z@ \raggedright
	\normalfont
	\interlinepenalty\@M
	\begin{center}
	\Large \bfseries\MakeTextUppercase{#1}\par\nobreak
	\end{center}
	\vskip 20\p@ % originally 40
}}

\renewcommand\section{%
	\@startsection {section}{1}{\z@}%
	{-4.7ex \@plus -1ex \@minus -.2ex}% space above heading
	{0.9ex \@plus.2ex}% space below heading
	{\normalfont\large\bfseries}%
}
\renewcommand\subsection{%
	\@startsection{subsection}{2}{\z@}%
	{-4.25ex\@plus -1ex \@minus -.2ex}%
	{0.9ex \@plus .2ex}%
	{\normalfont\large\bfseries}%
}
%\renewcommand\subsubsection{%
%	\@startsection{subsubsection}{3}{\z@}%
%	{-4.25ex\@plus -1ex \@minus -.2ex}%
%	{0.9ex \@plus .2ex}%
%	{\normalfont\normalsize\bfseries}%
%}


%%%%%%%%%%%%%%%%%%%%%%HEADINGS%%%%%%%%%%%%%%%%%%%%%%

\def\ps@headings{
	\let\@oddfoot\@empty
	\def\@oddhead{\small\ifthenelse{\equal{\@althead}{}}{%
	\ifodd\value{page} 
        {\@facundergradstud} 
    \else                         
    \ifthenelse{\equal{\@candidatename}{}}
            {}
            {\@candidateshort \ifthenelse{\equal{\@candidatenametwo}{}}
                            {}
                            {, \@candidateshorttwo \ifthenelse{\equal{\@candidatenamethree}{}}
                                                    {}
                                                    {, and \@candidateshortthree}}}
    \fi    
	\hfil 
        \ifthenelse{\equal{\@appname}{}}{%
	\ifodd\value{page} \@degree\ (\@subjectshort)\ \else \rightmark\
	\fi}{\@appname} / \thepage}
	{\@althead\thepage}}
	\let\@mkboth\markboth
	\def\chaptermark##1{%
	\markright { ##1}} 
}

\def\ps@myheadings{ % used by abstractbox 
	\let\@oddfoot\@empty\let\@evenfoot\@empty
	\def\@evenhead{\thepage\hfil\slshape\leftmark}
	\def\@oddhead{\rightmark}
	\let\@mkboth\@gobbletwo
	\let\chaptermark\@gobble
	\let\sectionmark\@gobble
}

%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%


%%%%%%%%%%%%%%%%%%%%TABLE OF CONTENTS%%%%%%%%%%%%%%%%%%

\renewcommand\tableofcontents{
\if@draftmode\else
	\if@twocolumn
	\@restonecoltrue\onecolumn
	\else
	\@restonecolfalse
	\fi
 	\chapter*{\contentsname}
	\renewcommand{\@althead}{\hspace{0.35\textwidth}CONTENTS~(Cont.)\hfil}
	\pagestyle{headings}
       	\thispagestyle{empty}
	\@starttoc{toc}%
	\if@restonecol\twocolumn\fi
\fi
}

%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%


%%%%%%%%%%%%%%%%%%%%%%LIST OF FIGURES%%%%%%%%%%%%%%%%%%%
\renewcommand\listoffigures{
\if@draftmode\else
	\if@twocolumn
	\@restonecoltrue\onecolumn
	\else
	\@restonecolfalse
	\fi
	\chapter*{\listfigurename}
\renewcommand{\@althead}{\hspace{0.3\textwidth}LIST OF FIGURES~(Cont.)\hfil}
	\pagestyle{headings}
       	\thispagestyle{empty}
	\linespacing{1.3}
	\addcontentsline{toc}{chapter}{\MakeTextUppercase\listfigurename}
	\@starttoc{lof} % must go after addcontentsline
	\if@restonecol\twocolumn\fi
\fi
}
%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%


%%%%%%%%%%%%%%%%%%%%%%%LIST OF TABLES%%%%%%%%%%%%%%%%%%%
\renewcommand\listoftables{
\if@draftmode\else
	\if@twocolumn
	\@restonecoltrue\onecolumn
	\else
	\@restonecolfalse
	\fi
	\chapter*{\listtablename}
\renewcommand{\@althead}{\hspace{0.3\textwidth}LIST OF TABLES~(Cont.)\hfil}
	\pagestyle{headings}
       	\thispagestyle{empty}
	\linespacing{1.3}
	\addcontentsline{toc}{chapter}{\MakeTextUppercase\listtablename}
	\@starttoc{lot}%
	\if@restonecol\twocolumn\fi
\fi
}
%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%


%%%%%%%%%%%%%%%%%%%%%%%%APPENDICES%%%%%%%%%%%%%%%%%%%

\newcommand\appendices{
  \if@draftmode\else\newpage\mbox{}\vskip 9cm
  %\renewcommand{\@althead}{\@facgradstud\hfil\@degree\ (\@subject)\ / }
  \renewcommand{\@appname}{Appendices}
  \begin{center}{\Large\bf APPENDICES}\end{center}
  \par\fi
  \setcounter{chapter}{0}%
  \setcounter{section}{0}%
  \gdef\@chapapp{\appendixname}%
  \gdef\thechapter{\@Alph\c@chapter}}

\renewcommand\appendix{
  \if@draftmode\else\newpage\mbox{}\vskip 9cm
  %\renewcommand{\@althead}{\@facgradstud\hfil\@degree\ (\@subject)\ / }
  \renewcommand{\@appname}{Appendix}
  \begin{center}{\Large\bf APPENDIX}\end{center}
  \par\fi
  \setcounter{chapter}{0}%
  \setcounter{section}{0}%
  \gdef\@chapapp{\appendixname}%
  \gdef\thechapter{\@Alph\c@chapter}}
%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%


%%%%%%%%%%%%%%%%%%%%%%%BIBLIOGRAPHY%%%%%%%%%%%%%%%%%%%

\renewenvironment{thebibliography}[1]{
	\chapter*{\bibname}
  \renewcommand{\@appname}{}
  	\markright{\bibname}
	\addcontentsline{toc}{chapter}{\MakeUppercase\bibname}
\ifthenelse{\boolean{@natbibmode}}{%
	\linespacing{1.3}
  \list{\@biblabel{\arabic{NAT@ctr}}}{\@bibsetup{#1}%
    \setcounter{NAT@ctr}{0}}%
    \ifNAT@openbib
      \renewcommand\newblock{\par}
    \else
      \renewcommand\newblock{\hskip .11em \@plus.33em \@minus.07em}%
    \fi
    \sloppy\clubpenalty4000\widowpenalty4000
    \sfcode`\.=1000\relax
    \let\citeN\cite \let\shortcite\cite
    \let\citeasnoun\cite}{\list{\@biblabel{\@arabic\c@enumiv}}%
        {\settowidth\labelwidth{\@biblabel{#1}}%
       		\leftmargin\labelwidth%
            	\advance\leftmargin\labelsep%
            	\@openbib@code%
            	\usecounter{enumiv}%
            	\let\p@enumiv\@empty
            	\renewcommand\theenumiv{\@arabic\c@enumiv}}%
	\sloppy
	\clubpenalty4000
	\@clubpenalty \clubpenalty
	\widowpenalty4000%
	\sfcode`\.\@m
}}{\ifthenelse{\boolean{@natbibmode}}{%
\def\@noitemerr{%
  \PackageWarning{natbib}
     {Empty `thebibliography' environment}}%
  \endlist\vskip-\lastskip}{\def\@noitemerr
       {\@latex@warning{Empty `thebibliography' environment}}%
      \endlist}}
%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%


%%%%%%%%%%%%%%%%%%%%%%BIOGRAPHY%%%%%%%%%%%%%%%%%%%%%
\newcommand{\biography}{
\if@draftmode\else
    \ifthenelse{\equal{\@appname}{}}{}{\renewcommand{\@appname}{}}
    \chapter*{Biography}
	\linespacing{1.7}
  	\markright{Biography}
	\label{biography} % last page of thesis
	\addcontentsline{toc}{chapter}{BIOGRAPHY}
    \begin{tabbing}

%For first student

    \ifthenelse{\equal{\@candidatename}{}}
    {} 
    {{\bf NAME} \hspace{5.5cm}\=\@candidatetitle \@candidatename~~\@candidatesurname \\
	{\bf DATE OF BIRTH} \>\@dateofbirth \\
	{\bf PLACE OF BIRTH} \>\@placeofbirth \\
	{\bf INSTITUTIONS ATTENDED} \>\@firstdegreeinstitution,
	\@firstdegreeyears \\
	\>\hspace{1cm}\=\@firstdegree \\ 
	\>Mahidol University, \@years \\
	\>\>\@fulldegree\ (\@subjectshort) \\ 
    }

%For second student

    \ifthenelse{\equal{\@candidatenametwo}{}}
    {} 
    {\>\vspace{0.2cm} \\
    {\bf NAME} \hspace{5.5cm}\=\@candidatetitletwo \@candidatenametwo~~\@candidatesurnametwo \\
	{\bf DATE OF BIRTH} \>\@dateofbirthtwo \\
	{\bf PLACE OF BIRTH} \>\@placeofbirthtwo \\
	{\bf INSTITUTIONS ATTENDED} \>\@firstdegreeinstitutiontwo,
	\@firstdegreeyearstwo \\
	\>\hspace{1cm}\=\@firstdegreetwo \\ 
	\>Mahidol University, \@yearstwo \\
	\>\>\@fulldegree\ (\@subjectshort) \\ 
    }

%For third student

    \ifthenelse{\equal{\@candidatenamethree}{}}
    {} 
    {\>\vspace{0.2cm} \\
    {\bf NAME} \hspace{5.5cm}\=\@candidatetitlethree \@candidatenamethree~~\@candidatesurnamethree \\
	{\bf DATE OF BIRTH} \>\@dateofbirththree \\
	{\bf PLACE OF BIRTH} \>\@placeofbirththree \\
	{\bf INSTITUTIONS ATTENDED} \>\@firstdegreeinstitutionthree,
	\@firstdegreeyearsthree \\
	\>\hspace{1cm}\=\@firstdegreethree \\ 
	\>Mahidol University, \@yearsthree \\
	\>\>\@fulldegree\ (\@subjectshort) \\ 
    }

	\end{tabbing}

\fi
}
%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%

\pagestyle{headings}
\AtBeginDocument{\pagenumbering{roman}}

