from google.appengine.ext import db

class Listing(db.Model):
    acreage = db.StringProperty()
    address = db.PostalAddressProperty()
    details = db.TextProperty()
    geoPt = db.GeoPtProperty()
    image = db.LinkProperty()
    notes = db.TextProperty()
    price = db.StringProperty()
    resultsList = db.ReferenceProperty(ImportedListing)
    sqft = db.StringProperty()

class ImportedListing(db.Model):
    date = db.DateTimeProperty(auto_now_add=True)
        

    
    
