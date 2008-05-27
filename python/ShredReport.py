from BeautifulSoup import BeautifulSoup

def ShredMiniTable(block, key, info):
    start = block.find(text=key).parent

    while start.name != 'table': # Walk up to the enclosing table...
        start = start.parent

    # Shred each row into two tds. The first has the attribute, the second
    # has the value.
    #
    for row in start.findAll('tr'): 
        data = row.findAll('td')
        if len(data) == 2:
            if len(data[0].contents) == 1:
                attrib = str(data[0].contents[0].string).strip()
                value = str(data[1].string).strip()
                info[attrib] = value


def ShredReport(doc):
    doc = doc.replace('<HRsize="1">','')
    soup = BeautifulSoup(doc, selfClosingTags=['HRsize'])
    reportTitles = soup.findAll(text='Residential Client Detail Report')

    entries = []
    for title in reportTitles:
        info = {}

        reportStart = title.parent.parent.parent.parent
        
        firstInfo = reportStart.nextSibling.nextSibling

        lotAddress = firstInfo.find(text='Lot:&nbsp;')
        if lotAddress:
            lotAddress = lotAddress.parent.parent.parent

            address = lotAddress.findAll('td')[2].contents[0].string
            info['Address'] = address.replace('&nbsp;', ' ')

            info['Image'] = firstInfo.find('img')['src']

            ShredMiniTable(firstInfo, 'Status', info)
            ShredMiniTable(firstInfo, 'Beds', info)
            ShredMiniTable(firstInfo, 'List Price', info)
            ShredMiniTable(firstInfo, 'Style', info)
            ShredMiniTable(firstInfo, 'Appx. SQFT', info)
            ShredMiniTable(firstInfo, 'School Information:', info)
            ShredMiniTable(firstInfo, 'Area', info)

            nextInfo = firstInfo.nextSibling.nextSibling
            ShredMiniTable(nextInfo, 'Heating/Cooling', info)

            pictureBlock = firstInfo.find(text='See Additional Pictures').parent.parent.parent
            info['MorePictures'] = 'http://locator.nwmls.com' + pictureBlock['href']

            # Pull out the HTML for fun
            #
            details = str(reportStart)
            for section in reportStart.findNextSiblings('table'):
                if section.find(text='Residential Client Detail Report'):
                    break
                details = details + '<br/>' + str(section)
            info['Details'] = details

            entries.append(info)

    return entries

if __name__=='__main__':
    from pprint import pprint
    import sys
    
    f = open(sys.argv[1])
    doc = f.read()
    f.close()

    results = ShredReport(doc)
    pprint(results)
    print len(results)

    
