from elementtree.ElementTree import ElementTree, Element, SubElement
from geopy import geocoders

mapsKey = "ABQIAAAA-tOCnuAeUWwCZ-YwLax2lxR0t0OOatV8zNpozyD7Ey_HPYK3hRTpYfIqUbHep2FWj7ssB8fl8KjRXQ"
ve = geocoders.Google(mapsKey)

def ConvertToKML(report):
    root = Element(KML("kml"))
    root.attrib['xmlns'] = 'http://earth.google.com/kml/2.2'
    doc = SubElement(root, KML("Document"))
    for entry in report:
        placemark = SubElement(doc, KML("Placemark"))
        SubElement(placemark, KML("name")).text = entry['Address']
        SubElement(placemark, KML("description")).text = BuildDescription(entry)
        #SubElement(placemark, KML("description")).text = entry['Details']
        SubElement(SubElement(placemark, KML("Point")), KML("coordinates")).text = Geocode(entry['Address'])
    return ElementTree(root)

def BuildDescription(entry):
    descFormat = """
<img src='%(Image)s'/><br/>
%(List Price)s for %(Beds)s beds and %(Baths)s baths on %(Acreage)s acres.<br/>
<a href='%(MorePictures)s'>pictures</a>, 
<a href='http://www.redfin.com/search#search_location=%(Listing#)s'>redfin</a>
"""
    
    return descFormat % entry
        
def Geocode(address):
    for result in ve.geocode(address, False):
        place, (lng, lat) = result
        return "%.5f,%.5f,0" % (lat, lng)

def Indent(elem, level=0):
    i = "\n" + level*"  "
    if len(elem):
        if not elem.text or not elem.text.strip():
            elem.text = i + "  "
        for child in elem:
            Indent(child, level+1)
            if not child.tail or not child.tail.strip():
                child.tail = i
        if not elem.tail or not elem.tail.strip():
            elem.tail = i
    else:
        if level and (not elem.tail or not elem.tail.strip()):
            elem.tail = i

def KML(name):
    # return "{http://earth.google.com/kml/2.2}" + name # KML procs have bugs
    return name

if __name__=='__main__':
    from ShredReport import ShredReport
    import sys
    
    f = open(sys.argv[1])
    doc = f.read()
    f.close()

    print 'Shredding report...'
    report = ShredReport(doc)
    print 'Converting to KML...'
    results = ConvertToKML(report)
    print 'Indenting...'
    Indent(results.getroot())
    results.write(open(sys.argv[1]+'.kml', 'w'))
