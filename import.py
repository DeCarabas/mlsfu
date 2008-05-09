import os
import wsgiref.handlers
from google.appengine.ext import db
from google.appengine.ext import webapp
from google.appengine.api import urlfetch

import MlsModels
import ShredReport

mapsKey = "ABQIAAAA-tOCnuAeUWwCZ-YwLax2lxR0t0OOatV8zNpozyD7Ey_HPYK3hRTpYfIqUbHep2FWj7ssB8fl8KjRXQ"
mapsPrefix = "http://maps.google.com/maps/geo?"

def Geocode(address):
    url = mapsPrefix + address.replace(' ','+') + '&output=csv&key=' + mapsKey
    result = urlfetch.fetch(url)
    if result.status_code == 200:
        parts = result.content.split(',')
        return db.GeoPt(float(parts[2]), float(parts[3]))
    return None

def ImportListing(url):
    result = urlfetch.fetch(url)
    if result.status_code == 200:
        results = ShredReport.ShredReport(result.content)

        importedListing = MlsModels.ImportedListing()
        importedListing.put()

        for result in results:
            listing = Listing()

            listing.acreage = result['Acreage']
            listing.address = db.PostalAddress(result['Address'])
            listing.details = result['Details']
            listing.geoPt = Geocode(result['Address'])
            listing.image = result['Image']
            listing.price = result['List Price']
            listing.resultsList = importedListing
            listing.sqft = result['Appx. SQFT']
            
            listing.put()

class ImportHandler(webapp.RequestHandler):
    def get(self):
        

