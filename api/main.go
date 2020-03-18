package main

import (
	"encoding/json"
	"time"
	"os"
	"crypto/rand"
	"net/http"
	"fmt"
	"log"
	"github.com/gorilla/mux"
	"github.com/rs/cors"
)

var version string = "v1"

type Ping struct {
	RequestId string
	TimeStamp string
	Version string
}

func createUUID() (string) {
	buf := make([]byte, 16)

	if _, err := rand.Read(buf); err != nil {
		panic(err.Error())
	}

	return fmt.Sprintf("%x-%x-%x-%x-%x", buf[0:4], buf[4:6], buf[6:8], buf[8:10], buf[10:])
}

type newAPIHandler struct { }
func (eh *newAPIHandler) GetID(r *http.Request) (string) {
	vars := mux.Vars(r)
	return vars["id"]
}

func (eh *newAPIHandler) writeReply(id string, w http.ResponseWriter, r *http.Request) {
	w.Header().Set("Content-Type", "application/json; charset=UTF-8")

	if os.Getenv("API_VERSION") != "" {
		version = os.Getenv("API_VERSION")
	}

	msg := Ping{ 
		id,
		time.Now().Format(time.RFC850), 
		version}	

	json.NewEncoder(w).Encode(msg)
}

func (eh *newAPIHandler) getPingById(w http.ResponseWriter, r *http.Request) {
	id := eh.GetID(r)
	eh.writeReply(id,w,r)
}

func (eh *newAPIHandler) getPing(w http.ResponseWriter, r *http.Request) {
	id := createUUID()
	eh.writeReply(id,w,r)
}

func (eh *newAPIHandler) optionsHandler(w http.ResponseWriter, r *http.Request) {
	w.WriteHeader(200)
}

func main() {
	handler := newAPIHandler{}

	r := mux.NewRouter()
	apirouter := r.PathPrefix("/api").Subrouter()
	apirouter.Methods("GET").Path("/ping").HandlerFunc(handler.getPing)
	apirouter.Methods("GET").Path("/ping/{id}").HandlerFunc(handler.getPingById)
	apirouter.Methods("OPTIONS").Path("/ping").HandlerFunc(handler.optionsHandler)

	server := cors.Default().Handler(r)

	port := ":8081"
	fmt.Print("Listening on port", port)
	log.Fatal(http.ListenAndServe( port , server))
}
