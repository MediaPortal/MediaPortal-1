#!/usr/bin/env python

# $Id$

# $Log$
# Revision 1.1  2004/04/21 14:56:30  yamp
# no message
#
# Revision 1.1  2004/04/21 05:47:14  yamp
# no message
#
# Revision 1.16  2004/03/04 12:48:15  paul
# Added check for programming with equal begin and end time. Sesamstraat
# now works :-)
#
# Revision 1.15  2003/12/28 10:03:56  paul
# Added --slowdays option. Grabs --slowdays with full info and the rest
# with fast info.
#
# Revision 1.14  2003/11/06 07:12:36  paul
# Python barfed on a regexp with a recursion limit error. Reworked the
# regular expression.
#
# Revision 1.13  2003/11/02 07:54:45  paul
# Fix: loop continues after exception.
#
# Revision 1.12  2003/11/01 21:53:06  paul
# Blah.
#
# Revision 1.11  2003/09/30 11:27:03  paul
# 1) FIXED BUG: two programs with identical end times and close start times could
#    remove programming for the rest of the day.
# 2) FIXED BUG: Removing items from a list did not check for double entries. This
#    could cause the removal of a single program.
# 3) FIXED BUG: the cache counters are now updated correctly. Before, a
#    cached entry would not be counted anymore, now the cleaning and
#    thresholding is more functional. Caching still needs a rethinking.
# 4) MINOR: changed the output while fetching a little bit.
#
# Revision 1.10  2003/09/10 07:16:56  paul
# Long overdue indentation tab->space conversion
# Fixed bug in ampersand handling: &...; tags were allowed to include
# whitespace.
#
# Revision 1.9  2003/08/01 06:50:56  paul
# Config file quick fix
#
# Revision 1.8  2003/07/28 11:26:29  paul
# Small fixes for pages that cannot be downloaded.
#
# Revision 1.7  2003/07/23 11:19:05  paul
# Removed the 250 character limit because it sometimes broke by breaking
# at &amp;
#
# Revision 1.6  2003/07/15 07:08:46  paul
# Removed some of the debugging output.
#
# Revision 1.5  2003/07/14 22:33:46  paul
# More or less perfectly working. Have to clean up some of the debugging
# code.
#
# Revision 1.4  2003/07/13 17:37:15  paul
# Added most of the caching stuff.
#
# Revision 1.3  2003/07/10 06:47:25  paul
# Randomized the fetching order for detail information.
#
# Revision 1.2  2003/07/09 21:08:44  paul
# Fixed the Rembo&Rembo error. Added a little bit of documentation.
#
# Revision 1.1.1.1  2003/07/09 16:26:43  paul
# First working version
#

import re
import urllib
import getopt, sys
from string import replace, split, strip
import time
import random
import htmlentitydefs
import os.path
import pickle

# do extra debug stuff
debug = 1

try:
    import redirect
except:
    debug = 0
    pass

"""
This is yet another XMLTV grabber based on tvgids.nl. It relies heavily
on parsing the website and if anything changes on the site this script
will probably fail.

Note:
All this is created without any serious knowledge of python or how
to create xmltv stuff. If it works, great! If not, well, send me a
patch.

Some minor adaptation of category stuff is made to please mythtv.
""" 


# globals

tvgids = 'http://www.tvgids.nl/'
uitgebreid_zoeken = tvgids + 'uitgebreidzoeken.html'


# Wait a random number of seconds between each page fetch.
# We want to be nice to tvgids.nl
# Also, it appears tvguid.nl throttles its output.
# So there.
nice_time = [1, 3]

# Create a category translation dictionary
# Look in mythtv/themes/blue/ui.xml for all categories
# The keys are the categories used by tvgids.nl
cattrans = { 'Film'             : 'Movies',
             'Amusement'        : 'Talk',
             'Nieuws/actualiteiten' : 'News',
             'Informatief'      : 'Educational',
             'Comedy'           : 'Comedy',
             'Serie/soap'       : 'SERIE',
             'Misdaad'          : 'Crime/Mystery',
             'Sport'            : 'Sports',
             'Muziek'           : 'Music',
             'Documentaire'     : 'Documentary',
             'Erotiek'          : 'Adult',
             'Kunst/Cultuur'    : 'Educational',
             'Wetenschap'       : 'Science/Nature',
             'Jeugd'            : 'Kids',
             'Natuur'           : 'Nature',
             'Animatie'         : 'Kids',
             'Religieus'        : 'Religion'}
              

# build inverse htmldefs dict
#html_to_latin1 = htmlentitydefs.entitydefs
#latin1_to_html = {}
#for key in html_to_latin1.keys():
#   latin1_to_html[html_to_latin1[key]] = key

# Work in progress, the idea is to cache program categories and
# descriptions to eliminate a lot of page fetches from tvgids.nl
# for programs that do not have interesting/changing descriptions

class ProgramCache:
    """
    A cache to hold program name and category info
    """
    def __init__(self, filename=None, threshold=3):
        """
        Nothing much to do
        """
        # if a program has been queried at threshold times with
        # the same information return the cached version
        self.threshold = threshold
        self.filename  = filename

        if filename == None:
            self.pdict = {}
        else:
            if os.path.isfile(filename):
                self.load(filename)
            else:
                self.pdict = {}


    def load(self, filename):
        """
        Loads a pickled cache dict from file
        """
        self.pdict = pickle.load(open(filename,'r'))

    def dump(self, filename):
        """
        Dumps a pickled cache
        """
        pickle.dump(self.pdict, open(filename, 'w'))

    
    def query(self, program_name):
        """
        Updates/gets/whatever.
        """

        retval = None

        for key in self.pdict.keys():
            item = self.pdict[key]
            if program_name == item[0] and item[3] >= self.threshold:
                retval = item
                break
        return retval

    def add(self, program_name, category, description):
        """
        Adds a program_name, category, description tuple
        """
        key = (program_name, category, description)
        if self.pdict.has_key(key):
            self.pdict[key][3] += 1
        else:
            self.pdict[key] = [program_name, category, description, 1]

    def clean(self):
        """
        Removes all programming under the current threshold from the cache
        """
        for key in self.pdict.keys():
            item = self.pdict[key]
            if item[3]<self.threshold:
                del self.pdict[key]

    def info(self):
        """
        Prints some info on the cache
        """
        n = len(self.pdict.keys())
        t = 0.0

        for key in self.pdict.keys():
            kaas = self.pdict[key]
            t += self.pdict[key][3]
            print "%s %s" % (kaas[3], kaas[0])
            
        print "Average threshold %s %s %s" % (t, n, t/n)




def usage():
    print 'Yet another grabber for Dutch television'
    print 'Usage:'
    print '--help, -h    = print this info'
    print '--days        = # number of days to grab'
    print '--slow        = also grab descriptions of programming'
    print '--slowdays    = grab slowdays initial days and the rest in fast mode'
    print '--offset      = # day offset from where to grab (0 is today)'
    print '--configure   = create configfile (overwrites existing file)'
    print '--output      = file where to put the output'
    print '--cache       = cache descriptions and use the file to store'
    print '--threshold # = number above which an item is considered to be cached'
    print '--clean_cache = clean the cache file and exit'
    print '--cache_info  = print some statistics about the cache and exit'


def filter_line(s):
    """
    Removes unwanted stuff in strings (copied from tv_grab_be)
    """

    # do the latin1 stuff
    tag = re.compile('(&\S*?;)')
    m   = tag.finditer(s)
    for match in m:
        s = replace(s,match.group(1), htmlentitydefs.entitydefs[match.group(1)])

    s = replace(s,'&nbsp;',' ')
    s = replace(s,'\r',' ')
    x = re.compile('(<.*?>)')
    s = x.sub('', s)

    # A couple of characters which are not legal in Latin-1, we have
    # to guess what they are.
    #
    s = replace(s, '~Q', "'")
    s = replace(s, '~R', "'")

    # Hmm, not sure if I understand this. Without it, mythfilldatabase barfs
    # on program names like "Steinbrecher &..."
    s = replace(s,'&','&amp;')

    return s

def delete_from_list(list, items):
    """
    Removes the indices in items from the list

    ['a', 'b', 'c', 'd' ] [2,1] -> ['a', 'd']
    """

    # unique-ify, sort, and reverse the range so that we start removing from the
    # back of the list
    u = {}
    for item in items:
        u[item] = 1;
    
    keys = u.keys()
    keys.sort()
    keys.reverse()
    for i in keys:
        del list[i]
    

def time_kludge(s):
    """
    Given a string of "HH:MM" returns a list with hh and mm

    "23:10" -> [23, 10]
    """
    hh = int(s[0:2])
    mm = int(s[3:5])
    return [hh, mm]
    
def duration(h1,m1,h2,m2):
    """
    Calculates the duration of a program (24h times)
    in minutes. [h2,m2] can be on the next day.

    duration(23,10,23,15) -> 5
    duration(23,10,0,20)  -> 70
    """
    if h2<h1:
        hd = 23-h1 + h2
        md = 60-m1 + m2
    else:
        hd = h2-h1
        md = m2-m1
        if md<0:
            md = 60+md
            hd = hd - 1
    return hd*60+md

def get_page(url):
    """
    Retrieves the url and returns a string with the contents
    """
    try:
        fp = urllib.urlopen(url)
        lines = fp.readlines()
        page = "".join(lines)
        return page
    except:
        sys.stderr.write('Cannot open url: %s\n' % url)
        sys.exit(10);
        return None


def get_channels(file):
    """
    Get a list of all available channels and store these
    in a file.
    """
    # store channels in a dict
    channels = {}

    # tvgids stores several instances of channels, we want to
    # find all the possibile channels
    channel_get = re.compile('<select.*?name="station">(.*?)</select>', \
                             re.DOTALL)

    # this is how we will find a (number, channel) instance
    channel_re  = re.compile('<option value="([0-9]+)">(.*?)</option>', \
                             re.DOTALL)

    # this is where we will try to find our channel list
    total = get_page(uitgebreid_zoeken)
    if total == None:
        return

    # get a list of match objects of all the <select blah station>
    stations = channel_get.finditer(total)

    # and create a dict of number, channel_name pairs
    # we do this this way because several instances of the 
    # channel list are stored in the url and not all of the 
    # instances have all the channels, this way we get them all.
    for station in stations:
        m = channel_re.finditer(station.group(0))           
        for p in m:
            a = int(p.group(1))
            b = p.group(2)
            # this is not naughty, tvgids starts with language selections
            # from 100 onwards, channel 0 does not exist
            if a>0 and a<100:
                channels[a] = b

    # sort on channel number (arbitrary but who cares)
    keys = channels.keys()
    keys.sort()

    # and create a file with the channels
    f = open(file,'w')
    for k in keys:
        f.write("%s %s\n" % (k, channels[k]))
    f.close()


def get_channel_day(channel, offset):
    """
    Get a day of programming for channel number, with
    an offset, where offset is one of (tvgids contains 
    no info beyond a certain day, 5 seems a reasonable upper limit
    0 = today 
    1 = tomorrow 
    2 = day after tomorrow etc

    The output is a list of programming in order where each row
    contains:
    [ start_time, stop_time, detail_url, program_name ]
    """


    channel_url = 'http://www.tvgids.nl/zoekprogramma.php?'+\
                  'trefwoord=Titel+of+trefwoord&'+\
                  'station=%s&genre=alle&interval=%s&timeslot=0&periode=0&order=0' % (channel, offset)


    # get the raw programming for a single day
    total = get_page(channel_url)
    if total == None:
        return

    # check for following pages on the url and fetch those too

    # get the url given for the next page on this page
    following_page = re.compile('<td\s+class="lijst_zender".*?<[aA].*?[hH][rR][Ee][fF]="(.*?)">Volgende</[Aa]>\s*</td>')

    all_pages_done = 0
    extra_page = total

    while not all_pages_done:
        # check for following page
        follow = following_page.search(extra_page)
        if follow != None:
            try:
                zoek = follow.group(1).strip()
                new_url = tvgids + zoek
                extra_page = get_page(new_url)
                total = total + extra_page
            except:
                all_pages_done = 1
                pass
        else:
            all_pages_done = 1
                
    # Setup a number of regexps

    # match  <tr>.*</tr> 
    getrow = re.compile('<[tT][Rr]>(.*?)</[tT][rR]>',re.DOTALL)

    # match the required program info
    # 1 = channel name
    # 2 = times
    # 3 = program name
    # 4 = url for details
    parserow = re.compile('class="lijst_zender">(.*?)</td>.*?'  +\
                          'class="lijst_tijd">(.*?)</td>.*?'    +\
                          'class="lijst_programma">.*?<[aA]\s+' +\
                          '[hH][rR][eE][fF]="(/.*?)".*?'        +\
                          'class="details_programma">(.*?)</[aA]', re.DOTALL)

    # tvgids.nl uses the following system:
    # 
    # 1) normal begin and end times
    times = re.compile('([0-9]+:[0-9]+)-([0-9]+:[0-9]+)')

    # 2) a program that starts at the end of another program
    #    (i.e. no start time for second program)
    #    for example:
    #    SBS 6  22:15-00:00    Hart van Nederland
    #    SBS 6 -22:40          Trekking Lotto
    #    meaning that "Trekking Lotto" starts after "Hart van Nederland" 
    times_follow = re.compile('-([0-9]+:[0-9]+)')

    # 3) nightly progamming (this is even worse)
    #    SBS 6  02:15-00:00  Nachtprogrammering
    #    SBS 6 -00:00  Mobile Nights
    #    SBS 6 -00:00  Stem van Nederland
    #    SBS 6 -  Hart van Nederland
    #    not handled

    # and find relevant programming info
    allrows = getrow.finditer(total)

    programs = []

    for r in allrows:

        detail = parserow.search(r.group(1))

        if detail != None: 

            # default times
            start_time = None
            stop_time  = None

            # parse for begin and end times
            t  = times.search(detail.group(2))
            tf = times_follow.search(detail.group(2))

            if t != None:
                start_time = t.group(1)
                stop_time  = t.group(2)
            elif tf != None:
                stop_time = tf.group(1)
            else:
                # Well, here we reach wonderful
                # programming that is so important that
                # begin and end times are not given.
                # Skip.
                pass

            program_url  = tvgids + detail.group(3)
            program_name = detail.group(4)
            program_name = program_name.strip()

            # store time, name and detail url in a list 
            programs.append([start_time, stop_time, program_url, program_name])

    #if debug:
        #sys.stderr.write('get_channel output----------\n')
        #for program in programs:
            #sys.stderr.write('%s\n' % program)

    # done
    return programs

def parse_programs(programs, offset):
    """
    Parse a list of programs as generated by get_channel_day()  and
    convert begin and end times to xmltv compatible times.  

    Programs is a list where each row contains:
    [ start_time, stop_time, detail_url, program_name ]
    
    """

    # good programs
    good_programs = []


    for i in range(len(programs)):

        # The common case: start and end times are present and are not
	# equal to each other (yes, this can happen)
        if programs[i][0] != None and programs[i][1] != None and programs[i][0] != programs[i][1]:
            good_programs.append(programs[i])


        # Check for clumped programming
        elif programs[i][0] == None and programs[i][1] != None:
            # This is programming that follows directly
            # after the previous programming. 
            # As far as I can see MythTV does not cater for clumpidx, here
            # we concatenate the names, create one program and hope for the best
            try:
                # double check
                if good_programs[-1][1] == '00:00':
                    # set end time of previous program to
                    # end time of this program
                    good_programs[-1][1] = programs[i][1]
                    # and adjust program name
                    good_programs[-1][3] += ' [ + %s]' % programs[i][3]
            except:
                # good_programs was empty. Should
                # not happen. Oh well.
                pass
        # hmm both times are none, skip
        else:
            pass

    # adjust last program, this also has the ``benefit'' that it
    # skips allnight sex/mobile phne programming.
    try:
        if good_programs[-1][1] == '00:00' and good_programs[-1][0][0] == '0':
            del(good_programs[-1])
    except:
        pass

    # So, good programming contains a list of all correct
    # programming with filled in start and end times.
    # All we have to do now is to correct the times for xmltv and to
    # account for a day switch.

    ##if debug:
        #sys.stderr.write('parse_program: after first test\n')
        #for program in good_programs:
            #sys.stderr.write('%s\n' % program)

    
    # Check for `motherprograms' i.e. a name given to a grouping of
    # programs, which is not a program itself.
    # Two checks are performed: if begin times match then the first
    # is removed.
    # If the time difference between the start times + the duration
    # of the second program is smaller than the duration of the
    # first program then the first program is removed.
    to_remove = []
    for c in range(0, len(good_programs)-1):
        first = good_programs[c]
        next  = good_programs[c+1]

        # also check for duration
        start1 = time_kludge(first[0])
        stop1  = time_kludge(first[1])
        start2 = time_kludge(next[0])
        stop2  = time_kludge(next[1])
        duration1 = duration(start1[0], start1[1], stop1[0], stop1[1])
        duration2 = duration(start2[0], start2[1], stop2[0], stop2[1])
        duration3 = duration(start1[0], start1[1], start2[0], start2[1])
        
        # if begin times match, remove the first program
        if first[0] == next[0]: 
            to_remove.append(c)

        # e.g.:
        # 09:00-12:00 Z@ppelin
        # 09:03-09:10 Pingu
        # 09:10-09:15 Whatever
        # the first is a mother program, but the start times do
        # not match
        elif (duration3+duration2)<duration1:
            to_remove.append(c)

	# if end times match, remove first program
        elif first[1] == next[1]:
	    to_remove.append(c)

    delete_from_list(good_programs, to_remove)

    # done with checks, now correct the times/dates

    # store enough dates so that an offset is an index into dates
    dates = [time.strftime('%Y%m%d', time.gmtime(time.time()+x*86400)) for x in range(0,offset+2)]

    # and finally, modify the start/end times to xmltv format
    add = 0
    prevstart = 0000
    prevstop  = 0000
    for c in range(len(good_programs)):
        start = good_programs[c][0].replace(':','')
        stop  = good_programs[c][1].replace(':','')

        # assign the new times
        good_programs[c][0] = dates[offset+add]+start+'00'
        good_programs[c][1] = dates[offset+add]+stop +'00'

        # check for day switch between programs
        if start<prevstop:
            add = 1
            good_programs[c][0] = dates[offset+add]+start+'00'
            good_programs[c][1] = dates[offset+add]+stop +'00'
            
        # check for day switch in program
        if int(stop) < int(start):
            add = 1
            good_programs[c][1] = dates[offset+add]+stop +'00'

        prevstart = start
        prevstop  = stop
    
    #if debug:
        #sys.stderr.write('parse done-------------\n')
        #for program in good_programs:
            #sys.stderr.write('%s\n' % program)
        
    # done, nothing to see here, please move on 
    return good_programs

def get_descriptions(programs, program_cache=None):
    """
    Given a programs list (from get_channel) 
    retrieve program information
    """

    detail = re.compile('<div class="detailDeel">.*?' +\
                        '<div class="detailLabel2">(.*?)</div>.*?'+\
                        '<div class="detailContent2">(.*?)</div>',\
                        re.DOTALL)
    
    num_descriptions = 0
    sys.stderr.write('Descriptions[%s]: ' % len(programs))

    # randomize detail requests
    fetch_order = range(0,len(programs))
    random.shuffle(fetch_order)

    #for i in range(0,len(programs)):
    for i in fetch_order:
        
        sys.stderr.write('\n%s: %s ' % (i, programs[i][3]))

        # add a dictionary to hold program details
        programs[i].append({})

        # if we have a cache use it
        if program_cache != None:
            cached_program = program_cache.query(programs[i][3])
            if cached_program != None:
                programs[i][-1]['Genre:']        = cached_program[1]
                programs[i][-1]['Beschrijving:'] = cached_program[2]
		# this adds 1 to the cache usage counter
		# silly structure, need to rethink the caching.
                program_cache.add(programs[i][3], cached_program[1], cached_program[2])
		sys.stderr.write('cached(%s) ' % cached_program[3]);
                #sys.stderr.write('Using cache(%s) ' %i)
                continue
                    
        # be nice to tvgids.nl
        time.sleep(random.randint(nice_time[0], nice_time[1]))

        # get the details page, and get all the detail nodes
        try:
            total = get_page(programs[i][2])
            descriptions = detail.finditer(total)
        except:
            # if we cannot find the description page, 
	    # go to next in the loop
            sys.stderr.write('Oh, thingy\n')
	    continue
            

        # now parse the details
        for description in descriptions:
            title   = description.groups()[0].strip()
            content = description.groups()[1].strip()
            content = filter_line(content)
            if content == '':
                continue

            elif title == '':
                programs[i][-1]['Beschrijving:'] = content
                #if len(content)<250:
                    #programs[i][-1]['Beschrijving:'] = content
                #else:
                    #programs[i][-1]['Beschrijving:'] = content[0:250]+"..."

            elif title == 'Genre:':
                try:
                    programs[i][-1][title] = cattrans[content]
                except:
                    programs[i][-1][title] = content
            

            elif title in ['Inhoud:', 'Titel aflevering:']:
                programs[i][-1][title] = content
                #if len(content)<250:
                    #programs[i][-1][title] = content
                #else:
                    #programs[i][-1][title] = content[0:250]+"..."

        # update the cache if necessary
        if program_cache != None:
            try:
                category = programs[i][-1]['Genre:']
            except:
                category = ''
            try:
                description = programs[i][-1]['Beschrijving:']
            except:
                description = ''
            program_cache.add(programs[i][3], category, description)
                    

    sys.stderr.write('done...\n')
                    
    # done

      

def xmlefy_programs(programs, channel):
    """
    Given a list of programming (from get_channels())
    returns a string with the xml equivalent
    """
    output = []
    for program in programs:
        output.append('  <programme start="%s +0000" stop="%s +0000" channel="%s">\n' % (program[0], program[1], channel))
        output.append('    <title lang="nl">%s</title>\n' % filter_line(program[3]))
        if len(program)==5:
            try:
                output.append('    <sub-title lang="nl">%s</sub-title>\n' % program[4]['Titel aflevering:'])
            except:
                pass
            if program[4].has_key('Inhoud:') and program[4].has_key('Beschrijving:'):
                try:
                    output.append('    <desc lang="nl">%s\n%s</desc>\n' % (program[4]['Inhoud:'], program[4]['Beschrijving:']))
                except:
                    pass
            else:
                try:
                    output.append('    <desc lang="nl">%s</desc>\n' % program[4]['Inhoud:'])
                except:
                    pass
                try:
                    output.append('    <desc lang="nl">%s</desc>\n' % program[4]['Beschrijving:'])
                except:
                    pass
            try:
                output.append('    <category lang="nl">%s</category>\n' % program[4]['Genre:'])
            except:
                pass
                
        output.append('  </programme>\n')
    
    return "".join(output)

def main():

    # Parse command line options
    try:
        opts, args = getopt.getopt(sys.argv[1:], "h", ["help", "output=", 
                                                       "offset=", "days=", 
                                   "configure", "slow", 
                                   "cache=", "threshold=", 
                                   "clean_cache", "cache_info"])
    except getopt.GetoptError:
        usage()
        sys.exit(2)

    output      = None
    output_file = None
    offset      = 0
    days        = 7
    slow        = 0
    slowdays    = 2
    program_cache = None
    program_cache_file = 'program_cache'
    config_file = 'tv_grab_nl_pdb.conf'
    threshold   = 3
    clean_cache = 0
    cache_info  = 0

    # seed the random generator
    random.seed()


    for o, a in opts:
        if o in ("-h", "--help"):
            usage()
            sys.exit(1)
        if o == "--configure":
            sys.stderr.write('Creating config file: %s\n' % config_file)
            get_channels(config_file)
            sys.exit(2)
        if o == "--days":
            days = int(a)
        if o == "--offset":
            offset = int(a)
        if o == "--slow":
            slow = 1
        if o == "--slowdays":
	    slowdays = int(a)
            # slowdays implies slow == 0
	    slow = 0
        if o == "--clean_cache":
            clean_cache = 1
        if o == "--threshold":
            threshold = int(a)
        if o == "--cache":
            program_cache_file = a
        if o == "--cache_info":
            cache_info = 1
        if o == "--output":
            output_file = a
            try:
                output = open(output_file,'w')
                # and redirect output
                if debug:
                    debug_file = open('/tmp/kaas.xml','w')
                    blah = redirect.Tee(output, debug_file) 
                    sys.stdout = blah
                else:
                    sys.stdout = output
            except:
                sys.stderr.write('Cannot write to outputfile: %s\n' % output_file)
                sys.exit(10);



    # get configfile if available
    try:
        f = open(config_file,'r')
    except:
        sys.stderr.write('Config file not found.\n')
        sys.stderr.write('Re-run me with the --configure flag.\n')
        sys.exit(1)

    #check for cache
    program_cache = ProgramCache(program_cache_file, threshold)
    if clean_cache != 0:
        print "Cleaning the cache using threshold = %s\n" % threshold
        program_cache.clean()
        program_cache.dump(program_cache_file)
        sys.exit(0)
    elif cache_info != 0:
        print "Cache info"
        program_cache.info()
        sys.exit(0)


    # Go!
    channels = {}

    # Read the channel stuff
    for blah in f.readlines():
        blah = blah.lstrip()
        blah = blah.replace('\n','')
        if blah[0] != '#':
            channel = blah.split()
            channels[channel[0]] = " ".join(channel[1:])

    # channels are now in channels dict keyed on channel id

    # print header stuff
    print '<?xml version="1.0" encoding="ISO-8859-1"?>'
    print '<!DOCTYPE tv SYSTEM "xmltv.dtd">'
    print '<tv generator-info-name="Icky Pooh">'

    # first do the channel info
    for key in channels.keys():
        print '  <channel id="%s">' % key
        print '    <display-name lang="nl">%s</display-name>' % channels[key]
        print '  </channel>'

    num_chans = len(channels.keys())
    cur_day = -1

    # now loop over the days and get the programming
    for x in range(0, days):
        cur_day += 1
        sys.stderr.write('Day %s of %s\n' % (cur_day, days))
        cur_chan  = -1
        fluffy = channels.keys()
        random.shuffle(fluffy)
    
        for id in fluffy:
            cur_chan += 1
            sys.stderr.write('Channel %s of %s\n' % (cur_chan, num_chans))
	    try:
                info = get_channel_day(id, offset+x)
                blah = parse_programs(info, offset+x)
                if slow or (slowdays>0 and x<slowdays):
                    get_descriptions(blah, program_cache)
                print xmlefy_programs(blah, id)
            except:
	        sys.stderr.write('Could not get channel %s\n' % cur_chan)
	        sys.exit(10);
            # be nice to tvgids.nl
            time.sleep(random.randint(2,5))

    # print footer stuff
    print "</tv>"

    # close the outputfile if necessary
    if output != None:
        output.close()

    # save the cache if necessary
    if program_cache != None:
        program_cache.dump(program_cache_file)

# allow this to be a module
if __name__ == '__main__':
    main()

# vim:tw=0:et:sw=4
