
  Readme for CCCP.ax DirectShow Closed Caption parser filter project
  ==================================================================

    -----------------------------------------
    Notes for original code version (2.0.0.6)
    -----------------------------------------
    CCCP (Core Closed Captioning Parser) is a DirectShow filter 
    that extracts Closed Captioning data from MPEG2 video. 
    The data is normally used by the downstream filter to 
    render and mix the CC with main video to aid hearing
    or language-impaired users

    Closed Captioning MPEG2 Parser
    Original author: Zodiak
    Copyright (C) 2004 zodiak@dvbn

    Also more information on the SourceForge CCCP project is here:
    http://sourceforge.net/p/cccp/discussion/496070/thread/6f610e28/
    -----------------------------------------


    ------ Start of SourceForge project discussion page text -----------------------------

    This text block copied from http://sourceforge.net/p/cccp/discussion/496070/thread/6f610e28/

    "Welcome to cccp,
    
    The project is finally set up. There's still little here but you can get the
    source code. Eventually, if you becoume a member you should get a CVS access but
    for now you can get it just with regular browser.
    http://cvs.sourceforge.net/viewcvs.py/cccp/
    
    The code compiles with MSVS 2003. You sill also so called "BaseClasses",
    originally provided by Microsoft in DirectX 9.0 (my version is summer 2004).
    Reportedly DS is now a part of Win32 so I don't know if you can download
    BaseClasses independently. Please give it a try and report the results here to
    make life easier for new people that (hopefully) will come after you.
    Any help is appreciated so please volunteer, especially if you have already
    worked in SouceForge projects, since as far as SF is concerned I myself is the
    greenest newbie that doesn't have time to climb the whole learning curve.
    If you want to become a developer, you'll have to (1)make sure you can compile
    the project from the downloaded source code, (2) you must set up a CVS and be
    able to access it anonymously, (3) fix a bug or implement a useful feature and
    send me (or any future admin) a snapshot of your source tree (zip). The changes
    will be code-reviewed and you will be provided with commit CVS access.
    Most of you have probably come from here from the DVBN thread and this is a
    great place to learn about CC and CCCP. If you haven't heard of it check it out
    (you will have to register):
    http://dvbn.happysat.org/viewtopic.php?t=19184&start=180
    
    Finally, the project is released under GNU Library or Lesser Public License
    (LGPL) (lgpl). This means for those of you who want to use it in your non-GPL
    (especially commercial) projects, you may do so but you must share all fixes,
    additions to the CCCP that you develop along the way. Please don't get greedy--
    we gave you a favour by sharing our stuff for free so you can improve your
    commercial products-- make sure you return the favour.
    
    Best wishes,
    Kostya."  (Kostya Sebov, 2005)
    
    ------ End of SourceForge discussion page text -----------------------------
    
    -------------------------------------------------------------
    Notes for MediaPortal version, May 2015
    =============================================================
    Modified to add H.264 Closed Caption parsing 
    and converted to Unicode.
   
    Copyright (C) 2015 Team MediaPortal
    http://www.team-mediaportal.com
   
    This Program is free software; you can redistribute it and/or modify
    it under the terms of the GNU General Public License as published by
    the Free Software Foundation; either version 2, or (at your option)
    any later version.
   
    This Program is distributed in the hope that it will be useful,
    but WITHOUT ANY WARRANTY; without even the implied warranty of
    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
    GNU General Public License for more details.
   
    You should have received a copy of the GNU General Public License
    along with GNU Make; see the file COPYING.  If not, write to
    the Free Software Foundation, 675 Mass Ave, Cambridge, MA 02139, USA.
    http://www.gnu.org/copyleft/gpl.html
   
    H.264 parsing derived from 'ccextractor' code, credits below:
    =============================================================
    ccextractor, version 0.75
    -------------------------
    Authors: Carlos Fernández (cfsmp3), Volker Quetschke.
    Maintainer: cfsmp3
    
    Lots of credit goes to other people, though:
    McPoodle (author of the original SCC_RIP), Neuron2, 
    and others (see source code).
    
    Home: http://www.ccextractor.org
    
    Google Summer of Code 2014 students
    - Willem van iseghem
    - Ruslan KuchumoV
    - Anshul Maheshwari
    -----------------------------------------------

