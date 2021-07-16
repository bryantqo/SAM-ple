import '/geometry-web-worker.js'

const app = Elm.Main.init({
    flags: JSON.parse(localStorage.getItem('storage'))
  })

  app.ports.initMap.subscribe( () => {
      var vecSource = new ol.source.Vector();
      window.vecSource = vecSource;
      var vecLayer = new ol.layer.Vector({source: vecSource});
    var map = new ol.Map({
        target: 'map',
        layers: [
          new ol.layer.Tile({
            source: new ol.source.OSM()
          }),
          vecLayer
        ],
        view: new ol.View({
          center: ol.proj.fromLonLat([-95.41, 40.82]),
          zoom: 4
        })
      });
  });

  app.ports.upload.subscribe(storage => {
        
        geometry_web_worker.promptAndProcessShapefile().then(features => 
        { 
            //Features is an array of GeoJSON features
            debugger;
            if(features && features.features.length)
                app.ports.load.send(features.features[0]);
        });
  });

  app.ports.setShapes.subscribe(shapes => {
      debugger;
      var gj = new ol.format.GeoJSON()
      for(var feat of shapes)
      {
          //TODO from geojson
            var f = gj.readFeature(feat);
            vecSource.addFeature(f);
      }
  });